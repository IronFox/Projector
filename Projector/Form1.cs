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


		private Solution shownSolution = null;

		private void ShowSolution(Solution solution)
		{
			shownSolution = solution;
			solutionView.Nodes.Clear();
			if (solution == null)
			{
				solutionToolStripMenuItem.Enabled = false;
				buildSolutionButton.Enabled = false;
				openGeneratedSolutionToolStripMenuItem.Enabled = false;
				openGeneratedSolutionButton.Enabled = openGeneratedSolutionToolStripMenuItem.Enabled;
				tabSelected.Text = "Focused (none)";
				//tabSelected.ac
				return;
			}
			TreeNode tsolution = solutionView.Nodes.Add(solution.ToString());
			foreach (Project project in solution.Projects)
			{
				List<String> options = new List<string>();
				if (project == solution.Primary)
					options.Add("primary");
				if (project.PurelyImplicitlyLoaded)
					options.Add("implicit");
				string optionString = options.Count > 0 ? " (" + options.Fuse(",") + ")" : "";

				TreeNode tproject = tsolution.Nodes.Add(project.Name + optionString + " [" + project.Type + "]");
				foreach (var r in project.References)
				{
					TreeNode treference = tproject.Nodes.Add(r.Project.Name + (r.IncludePath ? " (include)" : ""));
				}

				if (project.Macros.Count() > 0)
				{
					TreeNode tmacros = tproject.Nodes.Add("Macros");
					foreach (var m in project.Macros)
						tmacros.Nodes.Add(m.Key + "=" + m.Value);
				}
				if (project.CustomManifests.Count() > 0)
				{
					TreeNode tmacros = tproject.Nodes.Add("Manifests");
					foreach (var m in project.CustomManifests)
						tmacros.Nodes.Add(m.FullName);
				}
				if (project.CustomStackSize > -1)
				{
					tproject.Nodes.Add("custom stack size (bytes): " + project.CustomStackSize);
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
					s.ScanFiles(solution,project);
					AddSourceFolder(tsource.Nodes.Add(s.root.name), s.root);
				}

				TreeNode ttarget = tproject.Nodes.Add("TargetNames");
				foreach (Platform platform in Enum.GetValues(typeof(Platform)))
				{
					if (platform == Platform.None)
						continue;
					bool isCustom;
					string t = project.GetReleaseTargetNameFor(platform, out isCustom);
					ttarget.Nodes.Add(platform + ": \"" + t + "\"" + (isCustom ? " (custom)" : ""));
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

			solutionToolStripMenuItem.Enabled = solution.Primary != null;
			buildSolutionButton.Enabled = solution.Primary != null;
			tabSelected.Text = solution.ToString();

			openGeneratedSolutionToolStripMenuItem.Enabled = PersistentState.GetOutPathFor(solution.Source) != null;
			openGeneratedSolutionButton.Enabled = openGeneratedSolutionToolStripMenuItem.Enabled;

		}

		//PersistentState.SolutionDescriptor solution = new PersistentState.SolutionDescriptor();

		Dictionary<string,Solution>	solutions = new Dictionary<string,Solution>();
		List<Solution> loadedSolutions = new List<Solution>();

		bool loadedViewLock = false;

		private void UpdateAllNoneCheckbox()
		{
			bool anyFalse = false;
			bool anyTrue = false;
			for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
				if (loadedSolutionsView.Items[i].Checked)
					anyTrue = true;
				else
					anyFalse = true;
			loadedViewLock = true;
			loadedSolutionsView.Items[0].Checked = anyTrue && !anyFalse;
			loadedViewLock = false;

		}

        private Solution LoadSolution(FileInfo file)
        {
			FlushLog();
			if (!file.Exists)
			{
				LogLine("Error: Unable to read solution file '" + file + "'");
				return null;
			}

			Solution solution;
			if (solutions.TryGetValue(file.FullName,out solution))
			{
				LogLine("Solution '" + file + "' already loaded");
				solution.Reload();
			}
			else
			{
				solution = Solution.LoadNew(file);
				if (solution == null)
				{ 
					LogLine("Error: Unable to read solution file '"+file+"'");
					return null;
				}
				solutions.Add(file.FullName,solution);
				ListViewItem item = loadedSolutionsView.Items.Add(solution.ToString());
				loadedSolutions.Add(solution);
				item.SubItems.Add(solution.Primary != null ? solution.Primary.ToString() : "");
				item.SubItems.Add(solution.Projects.Count().ToString());
				item.Checked = true;
				UpdateAllNoneCheckbox();
				
				//solutionListBox.Items.Add(solution,true);
				UpdateRecent();
				mainTabControl.SelectedTab = tabLoaded;

				//tabLoaded.Show();
				//mainTabControl.TabIndex = 1;

			}
			
		//	FlushLog();


			ReportAndFlush(solution);

            LogLine("Projects imported: " + solution.Projects.Count());
			return solution;
        }

		private void ReportAndFlush(Solution solution)
		{
			LogLine(solution+":");
			foreach (var message in solution.Events.Messages)
			{
				LogLine("* " + message.ToString());
			}
			bool anyIssues = false;
			foreach (var warning in solution.Events.Warnings)
			{
				anyIssues = true;
				LogLine("Warning: " + warning);
			}
			if (!anyIssues)
				LogLine(solution +": No issues");
			solution.Events.Clear();

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

			ToolTip tooltip = new ToolTip();
			//tooltip.ToolTipIcon = ToolTipIcon.Info;
			//tooltip.IsBalloon = true;
			tooltip.ShowAlways = true;
			

            foreach (var recent in PersistentState.Recent)
            {
				ToolStripItemCollection parent = collection;

                LinkLabel lrecent = new LinkLabel();
                lrecent.Top = top;
                //lrecent.Font;

                lrecent.Font = f;
                lrecent.Left = 20;
                lrecent.Text = recent.ToString();
                lrecent.Width = lrecent.PreferredWidth;
                recentSolutions.Controls.Add(lrecent);

				//Label lpath = new Label();
				//lpath.Text = "("+recent.File.FullName+")";
				//lpath.Height = lpath.PreferredHeight;
				//lpath.Width = lpath.PreferredWidth;
				////lpath.Top = top;
				//lpath.Top = lrecent.Top + (lrecent.Font.Height - lpath.Font.Height) * 2;
				
				////lpath.Font = pathFont;
				//lpath.Left = lrecent.Left + lrecent.Width;
				//recentSolutions.Controls.Add(lpath);

				//lrecent.AutoToolTip = true;
				//lrecent.ToolTipText = recent.File.FullName;
				tooltip.SetToolTip(lrecent,recent.File.FullName);

				top += lrecent.Height;

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
				item.AutoToolTip = true;
				item.ToolTipText = recent.File.FullName;
                item.Click += (sender, item2) => LoadSolution(recent.File);
                lrecent.Click += (sender, item2) => LoadSolution(recent.File);
				//lpath.Click += (sender, item2) => LoadSolution(recent.File);
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
			Solution solution = LoadSolution(new FileInfo(p));
			if (solution != null)
			{
				FileInfo outPath = PersistentState.GetOutPathFor(solution.Source);
				if (outPath != null && outPath.Directory.Exists)
					BuildCurrentSolution(solution, outPath);
					
			}
		}

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Program.End();
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (shownSolution == null)
				return;
            FileInfo outPath = PersistentState.GetOutPathFor(shownSolution.Source);
            if (outPath == null || !outPath.Directory.Exists)
            {
				string solutionName = shownSolution.Name + ".sln";
				DirectoryInfo preferred = shownSolution.Source.Directory.CreateSubdirectory(Project.WorkSubDirectory);
				outPath = new FileInfo( Path.Combine(preferred.FullName,solutionName) );


				//buildAtToolStripMenuItem_Click(sender, e);
				//return;
            }
			Solution.FlushSourceScans();
			shownSolution.Reload(); //refresh
			ShowSolution(shownSolution);

			BuildCurrentSolution(shownSolution, outPath);
		}



		private void BuildCurrentSolution(Solution solution, FileInfo outPath)
		{
			if (solution.Build(outPath, this.toolSet.SelectedItem.ToString(),overwriteExistingVSUserConfigToolStripMenuItem.Checked))
			{ 
				openGeneratedSolutionToolStripMenuItem.Enabled = true;
				openGeneratedSolutionButton.Enabled = true;
			}
        }

        private void buildAtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string name = Project.Primary.Name;
			Solution solution = shownSolution;
			if (solution == null)
				return;
            chooseDestination.Filter = "Solution | " + solution.Name + ".sln";
            chooseDestination.FileName = solution.Name + ".sln";
			DirectoryInfo preferred = solution.Source.Directory.CreateSubdirectory(Project.WorkSubDirectory);
			chooseDestination.InitialDirectory = preferred.FullName;
            if (chooseDestination.ShowDialog() == DialogResult.OK)
            {
                PersistentState.SetOutPathFor(solution.Source, new FileInfo(chooseDestination.FileName));
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
			Solution solution = shownSolution;
			OpenGeneratedSolution(solution);
		}

		private void OpenGeneratedSolution(Solution solution)
		{
			if (solution == null)
				return;
            FileInfo slnPath = PersistentState.GetOutPathFor(solution.Source);
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
			if (solution.visualStudioProcess == null || solution.visualStudioProcess.HasExited)
			{ 
				Process myProcess = new Process();
				myProcess.StartInfo.FileName = "devenv.exe"; //not the full application path
				myProcess.StartInfo.Arguments = "\""+slnPath.FullName+"\"";
				if (!myProcess.Start())
					LogLine("Error: Failed to start process");
				else
					solution.visualStudioProcess = myProcess;
				
			}
		}

        private void overwriteExistingVSUserConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            overwriteExistingVSUserConfigToolStripMenuItem.Checked = !overwriteExistingVSUserConfigToolStripMenuItem.Checked;
        }

		private void solutionListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{

		}

		private void generateSelectedButton_Click(object sender, EventArgs e)
		{
			Solution.FlushSourceScans();
			FlushLog();
			for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
			{
				if (loadedSolutionsView.Items[i].Checked)
				{
					Solution solution = loadedSolutions[i-1];
					FileInfo outPath = PersistentState.GetOutPathFor(solution.Source);
					if (outPath != null && outPath.Directory.Exists)
					{ 
						solution.Reload(); //refresh
						solution.Build(outPath,this.toolSet.SelectedItem.ToString(),false);
						ReportAndFlush(solution);
						if (solution == shownSolution)
							ShowSolution(solution);
					}
					else
						LogLine("Error: Cannot export '" + solution + "'. Out path is not known.");
				}
			}


		}

		private void openSelectedButton_Click(object sender, EventArgs e)
		{
			for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
			{
				if (loadedSolutionsView.Items[i].Checked)
				{
					Solution solution = loadedSolutions[i - 1];
					OpenGeneratedSolution(solution);
				}
			}

		}

		private void unloadSelectedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
			{
				if (loadedSolutionsView.Items[i].Checked)
				{
					loadedSolutionsView.Items.RemoveAt(i);
					Solution sol = loadedSolutions[i-1];
					if (shownSolution == sol)
						ShowSolution(null);
					solutions.Remove(sol.Source.FullName);
					loadedSolutions.RemoveAt(i-1);
					i--;
				}
			}
			UpdateAllNoneCheckbox();
		}

		private void loadedSolutionsView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			if (e.ItemIndex > 0 && e.ItemIndex <= loadedSolutions.Count)
			{
				ShowSolution(loadedSolutions[e.ItemIndex-1]);
			}
			else
				ShowSolution(null);

		}

		private void solutionToolStripMenuItem_Click(object sender, EventArgs e)
		{

		}

		private void loadedSolutionsView_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (loadedViewLock)
				return;
			int index = loadedSolutionsView.Items.IndexOf(e.Item);
			if (index == 0 && e.Item != null)
			{
				for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
				{ 
					var item = loadedSolutionsView.Items[i];
					if (item != null)
						item.Checked = e.Item.Checked;
				}
			}
			else
				UpdateAllNoneCheckbox();
		}
    }
}
