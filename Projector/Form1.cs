using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Projector
{
    public partial class ProjectView : Form
    {
        public ProjectView()
        {
            InitializeComponent();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        public OpenFileDialog OpenDialog { get { return openProjectDialog; } }

        

        private void loadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openSolutionDialog.ShowDialog() == DialogResult.OK)
            {
                LoadSolution(new FileInfo(openSolutionDialog.FileName));
            }
        }

		public void FlushLog()
		{
			log.Text = "";

		}


        public void LogLine(string line)
        {
            if (log.Text.Length > 0)
                log.Text += "\r\n";
            log.Text += line;
			if (log.Visible)
			{
				log.SelectionStart = log.TextLength;
				log.ScrollToCaret();
			}
        }

        private void AddSourceFolder(TreeNode tsource, Project.Source.Folder root)
        {
            foreach (var grp in root.groups)
            { 
                //TreeNode tgroup = tsource.Nodes.Add(grp.Key.name);
                foreach (var f in grp.Value)
                    tsource.Nodes.Add(f.Name+ " ("+grp.Key.name+")");
            }

            foreach (var sub in root.subFolders)
                AddSourceFolder(tsource.Nodes.Add(sub.name), sub);
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ProjectView_Load(object sender, EventArgs e)
        {
            PersistentState.Restore();
            if (PersistentState.Toolset != null)
                toolSet.SelectedIndex = toolSet.Items.IndexOf(PersistentState.Toolset);
            else
            {
                toolSet.SelectedIndex = 0;
                PersistentState.Toolset = toolSet.SelectedItem.ToString();
            }
        }

		PersistentState.SolutionDescriptor solution = new PersistentState.SolutionDescriptor();


        private bool LoadSolution(FileInfo file)
        {
			if (!file.Exists)
			{
				return false;

			}
            Project.FlushAll();
			FlushLog();

            LogLine("Importing '" + file.FullName + "'...");


			
            {
                var xreader = new XmlTextReader(file.FullName);
                //int slashAt = Math.Max(file.FullName.LastIndexOf('/'), file.FullName.LastIndexOf('\\'));
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xreader);

				XmlNode xsolution = xdoc.SelectSingleNode("solution");
				XmlNode xDomain = xsolution.Attributes.GetNamedItem("domain");
				if (xDomain != null)
				{
					solution = new PersistentState.SolutionDescriptor(file,xDomain.Value);
				}
				else
					solution = new PersistentState.SolutionDescriptor(file, null);
				

				PersistentState.MemorizeRecent(solution);
				UpdateRecent();



                XmlNodeList xprojects = xdoc.SelectNodes("solution/project");

                foreach (XmlNode xproject in xprojects)
                {
                    Project.Add(xproject, file, null);
                }
                Directory.SetCurrentDirectory(file.DirectoryName);
                xreader.Close();
            }

            Project p;
            while ((p = Project.GetNextUnloaded()) != null)
            {
                if (!p.HasPath && !p.FillPath(file))
                    continue;

                string filename = p.SourcePath.FullName;
                var xreader = new XmlTextReader(filename);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xreader);
                XmlNode xproject = xdoc.SelectSingleNode("project");
                p.Load(xproject);
                xreader.Close();
            }



            solutionView.Nodes.Clear();
            TreeNode tsolution = solutionView.Nodes.Add(solution.Name);
            foreach (Project project in Project.All)
            {
                TreeNode tproject = tsolution.Nodes.Add(project.Name + (project == Project.Primary ? " (primary)" : "") + " [" + project.Type+"]");
                foreach (var r in project.References)
                {
                    TreeNode treference = tproject.Nodes.Add(r.project.Name + (r.includePath ? " (include)" : ""));
                }

				if (project.Macros.Count() > 0)
				{ 
					TreeNode tmacros = tproject.Nodes.Add("Macros");
					foreach (var m in project.Macros)
						tmacros.Nodes.Add(m.Key+"="+m.Value);
				}
				if (project.CustomManifests.Count() > 0)
				{
					TreeNode tmacros = tproject.Nodes.Add("Manifests");
					foreach (var m in project.CustomManifests)
						tmacros.Nodes.Add(m.FullName);
				}
				if (project.CustomStackSize > -1)
				{
					tproject.Nodes.Add("custom stack size (bytes): "+project.CustomStackSize);
				}
				if (project.PreBuildCommands.Count() > 0)
				{
					TreeNode tcommands = tproject.Nodes.Add("Pre-Build Commands");
					foreach (var m in project.PreBuildCommands)
					{ 
						TreeNode tparameters = tcommands.Nodes.Add(m.locatedExecutable != null ? m.locatedExecutable.FullName : m.originalExecutable);
						foreach (var param in m.parameters)
							tparameters.Nodes.Add(param);
					}
				}

				TreeNode tsource = tproject.Nodes.Add("Sources");
				foreach (var s in project.Sources)
                {
                    s.ScanFiles();
                    AddSourceFolder(tsource.Nodes.Add(s.root.name), s.root);
                }
            }

			foreach (var message in Project.Messages)
			{
				LogLine("* "+message.ToString());
			}
			foreach (var warning in Project.Warnings)
            {
                LogLine("Warning: " + warning);
            }
            if (Project.Warnings.Count == 0)
                LogLine("No issues");
            LogLine("Projects imported: " + Project.All.Count());
            solutionToolStripMenuItem.Enabled = Project.Primary != null;
			buildSolutionButton.Enabled = Project.Primary != null;
			return true;
        }


        private void UpdateRecent()
        {
            var collection = recentSolutionsToolStripMenuItem.DropDown.Items;
            collection.Clear();


			//bool anyHaveDomains = false;
			//foreach (var recent in PersistentState.Recent)
			//	if (recent.Domain != null)
			//	{
			//		anyHaveDomains = true;
			//		break;
			//	}



            foreach (var recent in PersistentState.Recent)
            {
				ToolStripItemCollection parent = collection;
				
				//if (anyHaveDomains)
				//{
				//	string lookFor = recent.Domain ?? "<No Domain>";

				//	ToolStripMenuItem found = null;
				//	for (int i = 0; i < collection.Count; i++)
				//		if (collection[i].Text == lookFor)
				//		{ 
				//			found = (ToolStripMenuItem)collection[i];
				//			break;
				//		}
					
				//	if (found != null)
				//	{
				//		//ToolStripMenuItem child = (ToolStripMenuItem)(collection[index]);
				//		parent = found.DropDown.Items;
				//	}
				//	else
				//	{
				//		ToolStripMenuItem child = new ToolStripMenuItem(lookFor);
				//		parent = child.DropDown.Items;
				//		collection.Add(child);
				//	}
				//}
                ToolStripItem item = new ToolStripMenuItem(recent.ToString());
                item.Click += (sender, item2) => LoadSolution(recent.File);
                parent.Add(item);

            }
            collection.Add("-");
            {
                ToolStripItem item = new ToolStripMenuItem("Clear List");
                item.Click += (sender, item2) => { PersistentState.ClearRecent(); UpdateRecent(); };
                collection.Add(item);

            }
            recentSolutionsToolStripMenuItem.Enabled = PersistentState.Recent.Count() > 0;

        }

        private void toolSet_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ProjectView_Shown(object sender, EventArgs e)
        {
			statusStrip.Items[0].Text = "Persistent state stored in " + PersistentState.StateFile.FullName;

            UpdateRecent();

			string[] parameters = Environment.GetCommandLineArgs();
			if (parameters.Length > 1)
				if (LoadSolution(new FileInfo(parameters[1])))
				{
					FileInfo outPath = PersistentState.GetOutPathFor(solution.File);
					if (outPath != null && outPath.Directory.Exists)
						BuildCurrentSolution(outPath);
					
				}
				else
					LogLine("Error: Unable to read solution file '"+parameters[1]+"'");
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static string Relativate(DirectoryInfo dir, FileInfo file)
        {
            Uri udir = new Uri(dir.FullName + "\\");
            Uri ufile = new Uri(file.FullName);
            Uri urelative = udir.MakeRelativeUri(ufile);
            string path = urelative.ToString();
			path = path.Replace("%20", " ");
			path = path.Replace('/', '\\');
			return path;
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo outPath = PersistentState.GetOutPathFor(solution.File);
            if (outPath == null || !outPath.Directory.Exists)
            {
                buildAtToolStripMenuItem_Click(sender, e);
                return;
            }

            LoadSolution(solution.File); //refresh

			BuildCurrentSolution(outPath);
		}

		private void BuildCurrentSolution(FileInfo outPath)
		{
            LogLine("Exporting to " + outPath.FullName);

            PersistentState.Toolset = toolSet.SelectedItem.ToString();

            DirectoryInfo dir = outPath.Directory;
            //DirectoryInfo projectDir = Directory.CreateDirectory(Path.Combine(dir.FullName, ".projects"));
            List<Tuple<FileInfo, Guid, Project>> projects = new List<Tuple<FileInfo, Guid, Project>>();
            string toolset = this.toolSet.SelectedItem.ToString();
            toolset = toolset.Substring(0, toolset.IndexOf(".0"));

            Configuration[] configurations = new Configuration[]
            {
                new Configuration() {name = "Debug", platform = "Win32", isRelease = false },
                new Configuration() {name = "Debug", platform = "x64", isRelease = false },
                new Configuration() {name = "Release", platform = "Win32", isRelease = true },
                new Configuration() {name = "Release", platform = "x64", isRelease = true },
            };


            foreach (Project p in Project.All)
            {
                var rs = p.SaveAs(toolset, configurations);
                projects.Add(new Tuple<FileInfo, Guid, Project>(rs.Item1, rs.Item2, p));
            }
            StreamWriter writer = File.CreateText(outPath.FullName);

            writer.WriteLine();
            writer.WriteLine("Microsoft Visual Studio Solution File, Format Version " + toolset + ".00");
            writer.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
            Guid solutionGuid = Guid.NewGuid();
            foreach (var tuple in projects)
            {
                string path = tuple.Item1.FullName;
				//Relativate(dir, tuple.Item1);
                writer.WriteLine("Project(\"{" + solutionGuid + "}\") = \"" + tuple.Item3.Name + "\", \"" + path + "\", \"{"
                    + tuple.Item2 + "}\");");
                writer.WriteLine("EndProject");
            }
            writer.WriteLine("Global");
            writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            foreach (var config in configurations)
                writer.WriteLine("\t\t"+config.name+"|"+config.platform+" = "+config.name+"|"+config.platform+"");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var tuple in projects)
            {
                Guid guid = tuple.Item2;
                writer.WriteLine("\t\t{" + guid + "}.Debug|Win32.ActiveCfg = Debug|Win32");
                writer.WriteLine("\t\t{" + guid + "}.Debug|Win32.Build.0 = Debug|Win32");
                writer.WriteLine("\t\t{" + guid + "}.Debug|x64.ActiveCfg = Debug|x64");
                writer.WriteLine("\t\t{" + guid + "}.Debug|x64.Build.0 = Debug|x64");
                writer.WriteLine("\t\t{" + guid + "}.Release|Win32.ActiveCfg = Release|Win32");
                writer.WriteLine("\t\t{" + guid + "}.Release|Win32.Build.0 = Release|Win32");
                writer.WriteLine("\t\t{" + guid + "}.Release|x64.ActiveCfg = Release|x64");
                writer.WriteLine("\t\t{" + guid + "}.Release|x64.Build.0 = Release|x64");
            }
            writer.WriteLine("\tEndGlobalSection");

            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");

            writer.WriteLine("EndGlobal");
            writer.Close();
            LogLine("Export done.");
        }

        private void buildAtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string name = Project.Primary.Name;
            chooseDestination.Filter = "Solution | " + solution.Name + ".sln";
            chooseDestination.FileName = solution.Name + ".sln";
			chooseDestination.InitialDirectory = solution.File.DirectoryName;
            if (chooseDestination.ShowDialog() == DialogResult.OK)
            {
                PersistentState.SetOutPathFor(solution.File, new FileInfo(chooseDestination.FileName));
                buildToolStripMenuItem_Click(sender, e);
            }
        }
    }
}
