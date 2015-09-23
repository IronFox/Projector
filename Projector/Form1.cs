using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
            recentSolutions.Visible = false;
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
            while ((p = Project.GetNextToLoad()) != null)
            {
                if (!p.HasSource && !p.AutoConfigureSourcePath(file))
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
            TreeNode tsolution = solutionView.Nodes.Add(solution.ToString());
            foreach (Project project in Project.All)
            {
				List<String>	options = new List<string>();
				if (project == Project.Primary)
					options.Add("primary");
				if (project.PurelyImplicitlyLoaded)
					options.Add("implicit");
				string optionString = options.Count > 0 ? " ("+options.Fuse(",")+")":"";

                TreeNode tproject = tsolution.Nodes.Add(project.Name + optionString + " [" + project.Type+"]");
                foreach (var r in project.References)
                {
                    TreeNode treference = tproject.Nodes.Add(r.Project.Name + (r.IncludePath ? " (include)" : ""));
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

				TreeNode ttarget = tproject.Nodes.Add("TargetNames");
				foreach (Platform platform in Enum.GetValues(typeof(Platform)))
				{
					if (platform == Platform.None)
						continue;
					bool isCustom;
					string t = project.GetReleaseTargetNameFor(platform, out isCustom);
					ttarget.Nodes.Add(platform+": \""+t+"\""+(isCustom ? " (custom)":""));
				}


				if (project.IncludedLibraries.Count() > 0)
				{
					TreeNode tlibs = tproject.Nodes.Add("Libraries");
					foreach (var s in project.IncludedLibraries)
					{
						TreeNode tlib = tlibs.Nodes.Add(s.Name);
						if (s.Includes.Count > 0)
						{
							TreeNode tincs = tlib.Nodes.Add("Include");
							foreach (var inc in s.Includes)
							{
								if (inc.Item1.AlwaysTrue)
									tincs.Nodes.Add(inc.Item2.FullName);
								else
								{ 
									TreeNode tinc = tincs.Nodes.Add(inc.Item1.ToString());
									tinc.Nodes.Add(inc.Item2.FullName);
								}
							}
						}
						if (s.LinkDirectories.Count > 0)
						{
							TreeNode tgroup = tlib.Nodes.Add("LinkDirectories");
							foreach (var link in s.LinkDirectories)
							{
								if (link.Item1.AlwaysTrue)
									tgroup.Nodes.Add(link.Item2.FullName);
								else
								{ 
									TreeNode telement = tgroup.Nodes.Add(link.Item1.ToString());
									telement.Nodes.Add(link.Item2.FullName);
								}
							}
						}
						if (s.Link.Count > 0)
						{
							TreeNode tgroup = tlib.Nodes.Add("Link");
							foreach (var link in s.Link)
							{
								if (link.Item1.AlwaysTrue)
									tgroup.Nodes.Add(link.Item2);
								else
								{ 
									TreeNode telement = tgroup.Nodes.Add(link.Item1.ToString());
									telement.Nodes.Add(link.Item2);
								}
							}
						}
					}



				}
            }
			tsolution.Expand();

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

			openGeneratedSolutionToolStripMenuItem.Enabled = PersistentState.GetOutPathFor(file) != null;
			openGeneratedSolutionButton.Enabled = openGeneratedSolutionToolStripMenuItem.Enabled;
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



            recentSolutions.Controls.Clear();

            int top = 10;

            Font f;
            {
                Label title = new Label();

                f = new Font(title.Font.FontFamily, title.Font.Size * 1.2f);
                title.Font = f;
                title.Text = "Recent Solutions:";
                title.Left = 15;
                title.Top = top;
                top += title.Height;
                title.Width = title.PreferredWidth;
                recentSolutions.Controls.Add(title);
            }

            foreach (var recent in PersistentState.Recent)
            {
				ToolStripItemCollection parent = collection;

                LinkLabel lrecent = new LinkLabel();
                lrecent.Top = top;
                //lrecent.Font;

                lrecent.Font = f;
                lrecent.Left = 20;
                lrecent.Text = recent.ToString();
                top += lrecent.Height;
                lrecent.Width = lrecent.PreferredWidth;
                recentSolutions.Controls.Add(lrecent);
                lrecent.ForeColor = Label.DefaultForeColor;
                //lrecent.Cursor = Cursor.


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
                lrecent.Click += (sender, item2) => LoadSolution(recent.File);
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
				LoadFromParameter(parameters[1]);
        }

		private void LoadFromParameter(string p)
		{
			if (LoadSolution(new FileInfo(p)))
			{
				FileInfo outPath = PersistentState.GetOutPathFor(solution.File);
				if (outPath != null && outPath.Directory.Exists)
					BuildCurrentSolution(outPath);
					
			}
			else
				LogLine("Error: Unable to read solution file '"+p+"'");
		}

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Program.End();
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo outPath = PersistentState.GetOutPathFor(solution.File);
            if (outPath == null || !outPath.Directory.Exists)
            {
				string solutionName = solution.Name + ".sln";
				DirectoryInfo preferred = solution.File.Directory.CreateSubdirectory(Project.WorkSubDirectory);
				outPath = new FileInfo( Path.Combine(preferred.FullName,solutionName) );


				//buildAtToolStripMenuItem_Click(sender, e);
				//return;
            }

            LoadSolution(solution.File); //refresh

			BuildCurrentSolution(outPath);
		}


        struct Config
        {
            public readonly string Name;
            public readonly bool IsRelease,
                                    Deploy;
            public    Config(string name, bool isRelease, bool deploy)
            {
                Name = name;
                IsRelease = isRelease;
                Deploy = deploy;
            }
        }

		private void BuildCurrentSolution(FileInfo outPath)
		{
            LogLine("Writing solution to '" + outPath.FullName+"'");

            PersistentState.Toolset = toolSet.SelectedItem.ToString();

            DirectoryInfo dir = outPath.Directory;
            //DirectoryInfo projectDir = Directory.CreateDirectory(Path.Combine(dir.FullName, ".projects"));
            List<Tuple<FileInfo, Guid, Project>> projects = new List<Tuple<FileInfo, Guid, Project>>();
			int toolset;
			{ 
				string toolsetStr = this.toolSet.SelectedItem.ToString();
				toolsetStr = toolsetStr.Substring(0, toolsetStr.IndexOf(".0"));
				if (!int.TryParse(toolsetStr,out toolset))
				{
					LogLine("Internal Error: Unable to decode toolset-version from specified toolset '"+this.toolSet.SelectedItem+"'");
					return;
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


            foreach (Project p in Project.All)
            {

                var rs = p.SaveAs(toolset, configurations, overwriteExistingVSUserConfigToolStripMenuItem.Checked);
				LogLine("Project '" +p.Name+"' written to '"+rs.Item1.FullName+"'");
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
                writer.WriteLine("\t\t"+config+" = "+config+"");
            writer.WriteLine("\tEndGlobalSection");
            writer.WriteLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var tuple in projects)
            {
                Guid guid = tuple.Item2;
				foreach (var config in configurations)
				{
					writer.WriteLine("\t\t{" + guid + "}." + config + ".ActiveCfg = " + config);
					writer.WriteLine("\t\t{" + guid + "}." + config + ".Build.0 = " + config);

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
            writer.WriteLine("\tEndGlobalSection");

            writer.WriteLine("\tGlobalSection(SolutionProperties) = preSolution");
            writer.WriteLine("\t\tHideSolutionNode = FALSE");
            writer.WriteLine("\tEndGlobalSection");

            writer.WriteLine("EndGlobal");
            writer.Close();
            LogLine("Export done.");


			PersistentState.SetOutPathFor(solution.File,outPath);


			openGeneratedSolutionToolStripMenuItem.Enabled = true;
			openGeneratedSolutionButton.Enabled = true;
        }

        private void buildAtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string name = Project.Primary.Name;
            chooseDestination.Filter = "Solution | " + solution.Name + ".sln";
            chooseDestination.FileName = solution.Name + ".sln";
			DirectoryInfo preferred = solution.File.Directory.CreateSubdirectory(Project.WorkSubDirectory);
			chooseDestination.InitialDirectory = preferred.FullName;
            if (chooseDestination.ShowDialog() == DialogResult.OK)
            {
                PersistentState.SetOutPathFor(solution.File, new FileInfo(chooseDestination.FileName));
                buildToolStripMenuItem_Click(sender, e);
            }
        }


		delegate void HandleInputCall(string text);

		internal void HandleInput(string input)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.buildSolutionButton.InvokeRequired)
			{	
				HandleInputCall d = new HandleInputCall(HandleInput);
				this.Invoke(d, new object[] { input });
			}
			else
			{
				LoadFromParameter(input);
			}
		}

		private void ProjectView_FormClosed(object sender, FormClosedEventArgs e)
		{
			Program.End();
		}

		private void openGeneratedSolutionToolStripMenuItem_Click(object sender, EventArgs e)
		{
            FileInfo slnPath = PersistentState.GetOutPathFor(solution.File);
			if (slnPath == null)
			{
				LogLine("Error: Received null while trying to retrieve sln location for '"+solution+"'. This should never happen...");
				return;
			}
			if (!slnPath.Exists)
			{
				LogLine("Error: '"+slnPath.FullName+"' does not exist");
				return;
			}
			Process myProcess = new Process();
			myProcess.StartInfo.FileName = "devenv.exe"; //not the full application path
			myProcess.StartInfo.Arguments = "\""+slnPath.FullName+"\"";
			if (!myProcess.Start())
				LogLine("Error: Failed to start process");
		}

        private void overwriteExistingVSUserConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            overwriteExistingVSUserConfigToolStripMenuItem.Checked = !overwriteExistingVSUserConfigToolStripMenuItem.Checked;
        }
    }
}
