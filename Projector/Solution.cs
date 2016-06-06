using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Projector
{
	public class Solution
	{
		struct Config
		{
			public readonly string Name;
			public readonly bool IsRelease,
									Deploy;
			public Config(string name, bool isRelease, bool deploy)
			{
				Name = name;
				IsRelease = isRelease;
				Deploy = deploy;
			}
		}

		public ListViewItem ListViewItem {  get; set; }


		private Dictionary<string, Project> map = new Dictionary<string, Project>();
		private static Dictionary<string, Project> globalMap = new Dictionary<string, Project>();
		private List<Project> list = new List<Project>();
		private Queue<Project> unloaded = new Queue<Project>();

		/// <summary>
		/// Fetches a list of all currently loaded projects
		/// </summary>
		public IEnumerable<Project> Projects { get { return list; } }
		/// <summary>
		/// Currently chosen primary project. Can be null, but should not with a loaded solution
		/// </summary>
		public Project Primary { get; private set; }


		private PersistentState.SolutionDescriptor solutionDesc;

		public string Name { get { return solutionDesc.Name; } }
		public string Domain { get { return solutionDesc.Domain; } }
		public PersistentState.SolutionDescriptor Desc { get { return solutionDesc; } }

		public readonly FileInfo Source;
		public System.Diagnostics.Process visualStudioProcess;

		public Solution(FileInfo source)
		{
			this.Source = source;
			//this.Events = new EventLog();
		}


		public override string ToString()
		{
			return  solutionDesc.ToString();
		}

		/// <summary>
		/// Flushes all local data to prepare a new solution import
		/// </summary>
		public void Clear()
		{
			Primary = null;
			map.Clear();
			list.Clear();
			unloaded.Clear();
		}

		public void EnqueueUnloaded(Project project)
		{
			unloaded.Enqueue(project);
		}

		/// <summary>
		/// Dequeues the next not-loaded project from the queue
		/// </summary>
		/// <returns>Project to load, or null if the queue is empty</returns>
		public Project GetNextToLoad()
		{
			if (unloaded.Count == 0)
				return null;
			return unloaded.Dequeue();
		}

		private Project FetchKnownProject(string name)
		{
			Project p;
			if (map.TryGetValue(name, out p))
				return p;
			if (globalMap.TryGetValue(name, out p))
			{
				map.Add(name,p);
				list.Add(p);
				foreach (var r in p.References)
					FetchKnownProject(r.Project.Name);
				return p;
			}
			return null;
		}

		private Project CreateNewProject(string name)
		{
			Project p = new Project(name);
			map.Add(name, p);
			globalMap.Add(name,p);
			list.Add(p);
			unloaded.Enqueue(p);
			return p;
		}

		public Project GetOrCreateProject(string name)
		{
			Project p = FetchKnownProject(name);
			if (p != null)
				return p;
			p = CreateNewProject(name);
			return p;
		}

		public void ScanEmptySources()
		{
			foreach (Project p in list)
			{
				foreach (Project.Source s in p.Sources)
					s.ScanFiles(this, p);

			}
		}

		public void SetPrimary(Project p)
		{
			if (Primary != null)
			{
				p.Warn(this,"Overriding primary project (was " + Primary.Name + ")");
			}
			Primary = p;
		}

		/// <summary>
		/// Loads a new solution
		/// </summary>
		/// <param name="file">.solution file to load</param>
		/// <param name="newRecent">Set to true if the solution is not listed in recent solutions, false otherwise</param>
		/// <returns>New solution or null, if the specified file could not be loaded</returns>
		public static Solution LoadNew(FileInfo file, out bool newRecent)
		{
			Solution solution = new Solution(file);
			if (!solution.Reload(out newRecent))
				return null;
			return solution;
		}


		public bool Reload(out bool newRecent)
		{
			newRecent = false;
            if (!Source.Exists)
			{
				return false;
			}
			Clear();
			EventLog.Inform(this,null,"Loading '" + Source.FullName + "'...");


			{
				var xreader = new XmlTextReader(Source.FullName);
				//int slashAt = Math.Max(file.FullName.LastIndexOf('/'), file.FullName.LastIndexOf('\\'));
				XmlDocument xdoc = new XmlDocument();
				xdoc.Load(xreader);

				XmlNode xsolution = xdoc.SelectSingleNode("solution");
				XmlNode xDomain = xsolution.Attributes.GetNamedItem("domain");
				
				if (xDomain != null)
				{
					solutionDesc = new PersistentState.SolutionDescriptor(Source, xDomain.Value);
				}
				else
					solutionDesc = new PersistentState.SolutionDescriptor(Source, null);
				

				PersistentState.MemorizeRecent(solutionDesc, out newRecent);

				XmlNodeList xprojects = xdoc.SelectNodes("solution/project");

				foreach (XmlNode xproject in xprojects)
				{
					Project.Add(xproject, Source, this, null);
				}
				Directory.SetCurrentDirectory(Source.DirectoryName);
				xreader.Close();
			}

			Project p;
			while ((p = GetNextToLoad()) != null)
			{
				if (!p.HasSource && !p.AutoConfigureSourcePath(Source))
					continue;

				string filename = p.SourcePath.FullName;
				var xreader = new XmlTextReader(filename);
				XmlDocument xdoc = new XmlDocument();
				xdoc.Load(xreader);
				XmlNode xproject = xdoc.SelectSingleNode("project");
				try
				{
					p.Load(xproject, this);
				}
				catch (Exception e)
				{
					EventLog.Warn(this, p, e.ToString());
				}
				xreader.Close();
			}
			return true;
		}


		public bool Build(FileInfo outPath, string strToolset, bool overwriteExistingVSUserConfig)
		{
			EventLog.Inform(this,null,"Writing solution to '" + outPath.FullName+"'");

            //PersistentState.Toolset = toolSet.SelectedItem.ToString();

            DirectoryInfo dir = outPath.Directory;
            //DirectoryInfo projectDir = Directory.CreateDirectory(Path.Combine(dir.FullName, ".projects"));
            List<Tuple<FileInfo, Guid, Project>> projects = new List<Tuple<FileInfo, Guid, Project>>();
			int toolset;
			{
				string toolsetStr = strToolset;//this.toolSet.SelectedItem.ToString();
				toolsetStr = toolsetStr.Substring(0, toolsetStr.IndexOf(".0"));
				if (!int.TryParse(toolsetStr,out toolset))
				{
					EventLog.Warn(this,null,"Internal Error: Unable to decode toolset-version from specified toolset '" + strToolset + "'");
					return false;
				}
			}

            List<Configuration> configurations = new List<Configuration>();

			{
				Platform[] platforms = new Platform[]{
					Platform.x32,
					Platform.x64
				};
				Config[] names = new Config[]
				{
					new Config("Debug", false,false),
                    new Config("OptimizedDebug", true,false),
					new Config("Release", true,true)
				};


				foreach (var p in platforms)
					foreach (var n in names)
						configurations.Add(new Configuration(n.Name,p,n.IsRelease,n.Deploy));
			}
			//{
			//	new Configuration() {Name = "Debug", Platform = "Win32", IsRelease = false },
			//	new Configuration() {Name = "Debug", Platform = "x64", IsRelease = false },
			//	new Configuration() {Name = "Release", Platform = "Win32", IsRelease = true },
			//	new Configuration() {Name = "Release", Platform = "x64", IsRelease = true },
			//};


            foreach (Project p in list)
            {
				var rs = p.SaveAs(toolset, configurations, overwriteExistingVSUserConfig,this);
				EventLog.Inform(this,p,"Written to '"+rs.Item1.FullName+"'");
				projects.Add(new Tuple<FileInfo, Guid, Project>(rs.Item1, rs.Item2, p));
            }
            StreamWriter writer = File.CreateText(outPath.FullName);

            writer.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
			writer.WriteLine("# Visual Studio "+toolset);
			writer.WriteLine("VisualStudioVersion = " + toolset);
			writer.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
			//Guid solutionGuid = Guid.NewGuid();
			string typeGUID = "8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942";   //C++. see http://www.codeproject.com/Reference/720512/List-of-Visual-Studio-Project-Type-GUIDs

			foreach (var tuple in projects)
            {
                string path = tuple.Item1.FullName;
				//Relativate(dir, tuple.Item1);
                writer.WriteLine("Project(\"{" + typeGUID + "}\") = \"" + tuple.Item3.Name + "\", \"" + path + "\", \"{"
                    + tuple.Item2 + "}\"");
                writer.WriteLine("EndProject");
            }
            writer.WriteLine("Global");
            writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var config in configurations)
                writer.WriteLine("\t\t"+config+" = "+config+"");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
			List<string> lines = new List<string>();
            foreach (var tuple in projects)
            {
                string guid = tuple.Item2.ToString().ToUpper();
				foreach (var config in configurations)
				{
					lines.Add("\t\t{" + guid + "}." + config + ".ActiveCfg = " + config);
					lines.Add("\t\t{" + guid + "}." + config + ".Build.0 = " + config);
				}
				//	writer.WriteLine("\t\t{" + guid + "}.Debug|Win32.ActiveCfg = Debug|Win32");
				//writer.WriteLine("\t\t{" + guid + "}.Debug|Win32.Build.0 = Debug|Win32");
				//writer.WriteLine("\t\t{" + guid + "}.Debug|x64.ActiveCfg = Debug|x64");
				//writer.WriteLine("\t\t{" + guid + "}.Debug|x64.Build.0 = Debug|x64");
				//writer.WriteLine("\t\t{" + guid + "}.Release|Win32.ActiveCfg = Release|Win32");
				//writer.WriteLine("\t\t{" + guid + "}.Release|Win32.Build.0 = Release|Win32");
				//writer.WriteLine("\t\t{" + guid + "}.Release|x64.ActiveCfg = Release|x64");
				//writer.WriteLine("\t\t{" + guid + "}.Release|x64.Build.0 = Release|x64");
            }
			lines.Sort();
			foreach (var line in lines)
				writer.WriteLine(line);
            writer.WriteLine("\tEndGlobalSection");

            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");

            writer.WriteLine("EndGlobal");
            writer.Close();
			EventLog.Inform(this,null,"Export done.");


			PersistentState.SetOutPathFor(Source,outPath);
			return true;
        }


		internal static void FlushGlobalProjects()
		{
			globalMap.Clear();
		}
	}
}
