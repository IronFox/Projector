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
                LoadSolution(new FileEntry(openSolutionDialog.FileName));
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

		private void ResizeFont(Control.ControlCollection coll, float scaleFactor)
		{
			foreach (Control c in coll)
			{
				if (c.HasChildren)
				{
					ResizeFont(c.Controls, scaleFactor);
				}
				c.Font = new Font(c.Font.FontFamily.Name, c.Font.Size * scaleFactor);
			}
		}

		public  float FontScaleFactor
		{
			get
			{
				Graphics graphics = this.CreateGraphics();
				float dpiX = graphics.DpiX;
				return dpiX / 96.0f;
			}

		}

		public bool ForceOverwriteProjectFiles { get { return forceOverwriteProjectFilesToolStripMenuItem.Checked; } }

		private void ProjectView_Load(object sender, EventArgs e)
        {
			ResizeFont(this.Controls, FontScaleFactor);
			toolsetLabel.Size = toolsetLabel.PreferredSize;
			toolSet.Left = toolsetLabel.Right + 4;

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
			solution.ScanEmptySources();
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

				tproject.Nodes.Add("Path").Nodes.Add(  project.SourcePath.FullName );
				TreeNode tdepedencies = tproject.Nodes.Add("Dependencies");

				foreach (var r in project.References)
				{
					TreeNode treference = tdepedencies.Nodes.Add(r.Project.Name + (r.IncludePath ? " (include)" : ""));
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
						TreeNode tparameters = tcommands.Nodes.Add(m.locatedExecutable.Exists ? m.locatedExecutable.FullName : m.originalExecutable);
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

			openGeneratedSolutionToolStripMenuItem.Enabled = PersistentState.GetOutPathFor(solution.Source).Exists;
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
				if (loadedSolutionsView.Items[i] != null && loadedSolutionsView.Items[i].Checked)
					anyTrue = true;
				else
					anyFalse = true;
			loadedViewLock = true;
			loadedSolutionsView.Items[0].Checked = anyTrue && !anyFalse;
			loadedViewLock = false;

		}

		private void LoadDomain(String domainName)
		{
			BeginLogSession();
			var list = PersistentState.Recent.ToArray();
			foreach (var recent in list)
			{
				if (recent.Domain ==domainName)
					LoadSolution(recent.File,true);
			}
			mainTabControl.SelectedTab = tabLoaded;
			UpdateAllNoneCheckbox();
			UpdateRecentAndPaths(true);
			EndLogSession();
		}


		int logDepth = 0;

		private void BeginLogSession()
		{
			logDepth++;
			Debug.Assert(logDepth <= 10);
			if (logDepth ==1)
				FlushLog();
		}

		private void EndLogSession()
		{
			logDepth--;
			Debug.Assert(logDepth >= 0);
			if (logDepth == 0)
				ReportAndFlush();
		}

		private void AbortLogSession(string errorMsg)
		{
			EventLog.Warn(null,null, errorMsg);
			EndLogSession();
		}

		private Solution LoadSolution(FileEntry file, bool batchLoad=false)
        {
			BeginLogSession();
			if (!file.Exists)
			{
				AbortLogSession("Error: Unable to read solution file '" + file + "'");
				return null;
			}
			bool newRecent;
			Solution solution;
			if (solutions.TryGetValue(file.FullName,out solution))
			{
				EventLog.Inform(solution,null, "Solution '" + file + "' already loaded");
				solution.Reload(out newRecent);

				RefreshListView(solution);
			}
			else
			{
				solution = Solution.LoadNew(file, out newRecent);
				if (solution == null)
				{ 
					AbortLogSession("Error: Unable to read solution file '"+file+"'");
					return null;
				}
				solutions.Add(file.FullName,solution);
				ListViewItem item = loadedSolutionsView.Items.Add(solution.ToString());
				loadedSolutions.Add(solution);
				item.SubItems.Add(solution.Primary != null ? solution.Primary.ToString() : "");
				item.SubItems.Add(solution.Projects.Count().ToString());
				item.Checked = true;
				solution.ListViewItem = item;

				//solutionListBox.Items.Add(solution,true);
				if (!batchLoad)
				{
					UpdateAllNoneCheckbox();
					if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
						mainTabControl.SelectedTab = tabLoaded;
					UpdateRecentAndPaths(newRecent);
				}

				//tabLoaded.Show();
				//mainTabControl.TabIndex = 1;

			}

			//	FlushLog();

			EventLog.Inform(solution, null, solution.Projects.Count().ToString()+ " project(s) imported");
			EndLogSession();

			return solution;
        }

		private string LogNextEvent(ref Solution currentSolution, EventLog.Notification n)
		{
			string head0 = "", head1 = "";
			if (currentSolution != n.Solution)
			{
				if (n.Solution != null)
				{
					head0 += n.Solution.Name;
					if (n.Project != null)
						head0 += "/";
				}
				currentSolution = n.Solution;
			}
			else
			{
				if (n.Solution != null)
				{
					//if (n.Project != null)
						//head0 += "/";
					head1 ="  ";
				}
			}
			if (n.Project != null)
			{
				head0 += n.Project.Name;
			}
			if (head0.Length != 0)
				head0 += ": ";
			return head1 + head0 + n.Text; 
		}
		private void ReportAndFlush()
		{
			//LogLine(solution+":");
			Solution current = null;
			foreach (var message in EventLog.Messages)
			{
				LogLine(LogNextEvent(ref current , message));
			}
			LogLine("");
			bool anyIssues = false;
			foreach (var warning in EventLog.Warnings)
			{
				anyIssues = true;
				LogLine("Warning: " + warning);
			}
			if (!anyIssues)
				LogLine("No issues");
			EventLog.Clear();
			logLabel.Text = "Log (" + EventLog.LastEvent.ToLongTimeString() + ")";
		}


		class RecentItems
		{
			List<LinkLabel> allLabels = new List<LinkLabel>();
			Dictionary<string, List<LinkLabel>> domainMap = new Dictionary<string, List<LinkLabel>>();
			Font defaultFont, litFont;

			public void Clear()
			{
				allLabels.Clear();
				domainMap.Clear();
			}

			public void Add(string domain, LinkLabel label)
			{
				if (defaultFont == null)
				{
					defaultFont = label.Font;
					litFont = new Font(defaultFont, FontStyle.Italic);
				}
				allLabels.Add(label);
				if (domain != null)
				{
					if (domainMap.ContainsKey(domain))
						domainMap[domain].Add(label);
					else
						domainMap.Add(domain, new List<LinkLabel>(new LinkLabel[]{ label }));
				}
			}

			internal void ClearHighlight()
			{
				foreach (var label in allLabels)
				{ 
					label.Font = defaultFont;
					label.Width = label.PreferredWidth;
				}
			}

			internal void HighlightDomain(string domain)
			{
				ClearHighlight();
				List<LinkLabel> hightlighted;
				if (domainMap.TryGetValue(domain, out hightlighted))
					foreach (var label in hightlighted)
					{
						label.Font = litFont;
						label.Width = label.PreferredWidth;
					}
			}
		}

		RecentItems recentItems = new RecentItems();

        private void UpdateRecentAndPaths(bool recentListChanged)
        {
			var fileList = locationOfProjectFileToolStripMenuItem.DropDown.Items;
			fileList.Clear();
			bool any = false;
			foreach (string name in PathRegistry.GetAllProjectNames())
			{
				ToolStripItem item = new ToolStripMenuItem(name);
				item.Click += (sender, item2) => { PathRegistry.UnsetPathFor(name); UpdateRecentAndPaths(false); };
				fileList.Add(item);
				any = true;


			}

			locationOfProjectFileToolStripMenuItem.Enabled = any;

			var collection = recentSolutionsToolStripMenuItem.DropDown.Items;
            collection.Clear();


			//bool anyHaveDomains = false;
			//foreach (var recent in PersistentState.Recent)
			//	if (recent.Domain != null)
			//	{
			//		anyHaveDomains = true;
			//		break;
			//	}


			bool rebuildPanel = recentListChanged || mainTabControl.SelectedTab != tabRecent;

			int top = 10;
			Font f = null;
			if (rebuildPanel)
			{
				recentSolutions.Controls.Clear();
				recentItems.Clear();


				{
					Label title = new Label();

					f = new Font(title.Font.FontFamily, title.Font.Size * 1.2f * FontScaleFactor);
					title.Font = f;
					title.Text = "Recent Solutions: (shift+click to load but stay on this tab)";
					title.Left = 15;
					title.Top = top;
					top += title.Height;
					title.Width = title.PreferredWidth;
					recentSolutions.Controls.Add(title);
				}

			}

			ToolTip tooltip = new ToolTip();
			tooltip.ShowAlways = true;
			int left = 20;
			//string lastDomain = null;
			foreach (var recent in PersistentState.Recent)
			{
				ToolStripItemCollection parent = collection;

				if (rebuildPanel)
				{
					//int lineHeight = 0;
					bool hasDomain = recent.Domain != null && recent.Domain.Length != 0;
					if (hasDomain)//&& lastDomain != recent.Domain
					{
						LinkLabel ldomain = new LinkLabel();
						ldomain.Top = top;

						ldomain.Font = f;
						ldomain.Left = 20;
						ldomain.Text = recent.Domain+"/";
						ldomain.Width = ldomain.PreferredWidth;
						left = 20 + ldomain.Width;
						recentSolutions.Controls.Add(ldomain);
						tooltip.SetToolTip(ldomain, "Load all recent projects of domain '"+recent.Domain+"'");
						ldomain.MouseEnter += (sender, item2) => recentItems.HighlightDomain(recent.Domain);
						ldomain.MouseLeave += (sender, item2) => recentItems.ClearHighlight();
						//top += lrecent.Height;
						ldomain.Click += (sender, item2) => LoadDomain(recent.Domain);
					}
					else
						left = 20;
					//lastDomain = recent.Domain;

					{ 
						LinkLabel lrecent = new LinkLabel();
						lrecent.Top = top;
						//lrecent.Font;

						lrecent.Font = f;
						lrecent.Left = left;
						lrecent.Text = recent.Name;
						lrecent.Width = lrecent.PreferredWidth;
						recentSolutions.Controls.Add(lrecent);
						tooltip.SetToolTip(lrecent, recent.File.FullName);
						top += lrecent.Height;
						lrecent.Click += (sender, item2) => LoadSolution(recent.File);

						recentItems.Add(hasDomain ? recent.Domain : null, lrecent);
					}

				}

				ToolStripItem item = new ToolStripMenuItem(recent.ToString());
				item.AutoToolTip = true;
				item.ToolTipText = recent.File.FullName;
                item.Click += (sender, item2) => LoadSolution(recent.File);
				//lpath.Click += (sender, item2) => LoadSolution(recent.File);
                parent.Add(item);
            }
            collection.Add("-");
            {
                ToolStripItem item = new ToolStripMenuItem("Clear List");
                item.Click += (sender, item2) => { PersistentState.ClearRecent(); UpdateRecentAndPaths(true); };
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

            UpdateRecentAndPaths(true);

			string[] parameters = Environment.GetCommandLineArgs();
			if (parameters.Length > 1)
				LoadFromParameter(parameters[1]);
        }

		private void LoadFromParameter(string p)
		{
			Solution solution = LoadSolution(new FileEntry(p));
			if (solution != null)
			{
				FileEntry outPath = PersistentState.GetOutPathFor(solution.Source);
				if (outPath.DirectoryExists)
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
			FileEntry outPath = PersistentState.GetOutPathFor(shownSolution.Source);
            if (!outPath.DirectoryExists)
            {
				string solutionName = shownSolution.Name + ".sln";
				DirectoryInfo preferred = shownSolution.Source.Directory.CreateSubdirectory(Project.WorkSubDirectory);
				outPath = new FileEntry( Path.Combine(preferred.FullName,solutionName) );


				//buildAtToolStripMenuItem_Click(sender, e);
				//return;
            }
			FlushProjects();
			bool newRecent;
			shownSolution.Reload(out newRecent); //refresh
			ShowSolution(shownSolution);

			BuildCurrentSolution(shownSolution, outPath);
		}

		void FlushProjects()
		{
			foreach (Solution s in loadedSolutions)
			{
				s.Clear();
			}
			Solution.FlushGlobalProjects();
		}


		private void BuildCurrentSolution(Solution solution, FileEntry outPath)
		{
			if (solution.Build(outPath, this.toolSet.SelectedItem.ToString(),overwriteExistingVSUserConfigToolStripMenuItem.Checked))
			{
				if (solution == shownSolution)
				{
					openGeneratedSolutionToolStripMenuItem.Enabled = true;
					openGeneratedSolutionButton.Enabled = true;
				}

				RefreshListView(solution);
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
                PersistentState.SetOutPathFor(solution.Source, new FileEntry(chooseDestination.FileName));
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
			FileEntry slnPath = PersistentState.GetOutPathFor(solution.Source);
			if (slnPath.IsEmpty)
			{
				LogLine("Error: Out location unknown for '"+solution+"'. Chances are, this solution has not been generated.");
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

		private void Generate(Solution solution)
		{
			BeginLogSession();
			FileEntry outPath = PersistentState.GetOutPathFor(solution.Source);
			if (!outPath.Exists)
			{
				DirectoryInfo preferred = solution.Source.Directory.CreateSubdirectory(Project.WorkSubDirectory);
				if (preferred != null)
				{
					string outName = Path.Combine(preferred.FullName, solution.Name + ".sln");
					EventLog.Warn(solution,null,"Notify: Out path for '" + solution + "' not known. Defaulting to " + outName);
					outPath = new FileEntry(outName);
					PersistentState.SetOutPathFor(solution.Source, outPath);
				}
			}

			if (outPath.DirectoryExists)
			{
				bool newRecent;
				solution.Reload(out newRecent); //refresh
				solution.Build(outPath, this.toolSet.SelectedItem.ToString(), false);
				if (solution == shownSolution)
					ShowSolution(solution);

				RefreshListView(solution);
			}
			else
				EventLog.Warn(solution,null, "Error: Cannot export '" + solution + "'. Out path is not known.");
			EndLogSession();
		}

		private void RefreshListView(Solution solution)
		{
			ListViewItem item = solution.ListViewItem;
			if (item != null)
			{
				item.SubItems[2].Text = solution.Projects.Count().ToString();
			}
		}

		private void generateSelectedButton_Click(object sender, EventArgs e)
		{
			FlushProjects();
			BeginLogSession();
			for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
			{
				if (loadedSolutionsView.Items[i].Checked)
				{
					Solution solution = loadedSolutions[i-1];
					Generate(solution);
				}
			}
			EndLogSession();
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
				loadedViewLock = true;
				for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
				{ 
					var item = loadedSolutionsView.Items[i];
					if (item != null)
						item.Checked = e.Item.Checked;
				}
				loadedViewLock = false;
			}
			else
				UpdateAllNoneCheckbox();
		}

		private void buildSolutionButton_Click(object sender, EventArgs e)
		{
			FlushProjects();
			if (shownSolution == null)
			{
				LogLine("Error: No solution focused.");
				return;
			}
			Generate(shownSolution);
			ShowSolution(shownSolution);
		}

		private void toolSet_SelectedIndexChanged_1(object sender, EventArgs e)
		{
			PersistentState.Toolset = toolSet.SelectedItem.ToString();
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			new About().Show();
		}

		private void fileHistoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			PersistentState.ClearRecent();
			UpdateRecentAndPaths(true);
		}
		

		private void pathRegistryToolStripMenuItem_Click_1(object sender, EventArgs e)
		{
			PathRegistry.Clear();
			UpdateRecentAndPaths(false);
		}

		private void recentSolutions_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
				foreach (string file in files)
					if (!file.ToLower().EndsWith(".solution"))
						return;

				e.Effect = DragDropEffects.Copy;
			}
		}

		private void recentSolutions_DragDrop(object sender, DragEventArgs e)
		{
			BeginLogSession();
			string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
			foreach (string file in files)
			{
				LoadSolution(new FileEntry(file), true);

			}

			UpdateAllNoneCheckbox();
			if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift)
				mainTabControl.SelectedTab = tabLoaded;
			UpdateRecentAndPaths(true);
			EndLogSession();
		}

		private void forceOverwriteProjectFilesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			forceOverwriteProjectFilesToolStripMenuItem.Checked = !forceOverwriteProjectFilesToolStripMenuItem.Checked;
		}
	}
}
