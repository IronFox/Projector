using Microsoft.VisualStudio.Setup.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        private void MenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        public OpenFileDialog OpenDialog { get { return openProjectDialog; } }

        

        private void LoadProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openSolutionDialog.ShowDialog() == DialogResult.OK)
            {
                LoadSolution(new File(openSolutionDialog.FileName));
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
            //log.Text += line;
			log.AppendText(line);
			//if (log.Visible)
			//{
			//	log.SelectionStart = log.TextLength;
			//	log.ScrollToCaret();
			//}
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

        private void SplitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
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


		private static void PrintWorkloads(ISetupPackageReference[] packages)
		{
			var workloads = from package in packages
							where string.Equals(package.GetType(), "Workload", StringComparison.OrdinalIgnoreCase)
							orderby package.GetId()
							select package;

			foreach (var workload in workloads)
			{
				Console.WriteLine($"    {workload.GetId()}");
			}
		}
		private static void PrintInstance(ISetupInstance instance, ISetupHelper helper)
		{
			var instance2 = (ISetupInstance2)instance;
			var state = instance2.GetState();
			Console.WriteLine($"InstanceId: {instance2.GetInstanceId()} ({(state == InstanceState.Complete ? "Complete" : "Incomplete")})");

			var installationVersion = instance.GetInstallationVersion();
			var version = helper.ParseVersion(installationVersion);

			Console.WriteLine($"InstallationVersion: {installationVersion} ({version})");

			if ((state & InstanceState.Local) == InstanceState.Local)
			{
				Console.WriteLine($"InstallationPath: {instance2.GetInstallationPath()}");
			}

			if ((state & InstanceState.Registered) == InstanceState.Registered)
			{
				Console.WriteLine($"Product: {instance2.GetProduct().GetId()}");
				Console.WriteLine("Workloads:");

				PrintWorkloads(instance2.GetPackages());
			}

			Console.WriteLine();
		}

		private static string InstancePath(ISetupInstance instance, ISetupHelper helper, string name, string version)
		{
			var instance2 = (ISetupInstance2)instance;
			var state = instance2.GetState();

			var installationVersion = instance.GetInstallationVersion();
			if (!installationVersion.StartsWith(version))
				return null;
			//var version = helper.ParseVersion(installationVersion);

			Console.WriteLine($"InstallationVersion: {installationVersion} ({version})");

			if ((state & InstanceState.Local) != InstanceState.Local)
				return null;

			if ((state & InstanceState.Registered) == InstanceState.Registered)
			{
				if (!instance2.GetProduct().GetId().StartsWith(name))
					return null;
				string rs = Path.Combine(instance2.GetInstallationPath(), "Common7", "IDE");
				if (!Directory.Exists(rs))
					return null;
				return rs;
			}
			return null;
		}


		private static string TryGetVSSetupPath(string version)
		{
			var configuration = new SetupConfiguration();
			var en = configuration.EnumAllInstances();
			ISetupInstance[] inst = new ISetupInstance[1];
			int got;
			do
			{
				en.Next(1, inst, out got);
				if (got > 0)
				{
					string rs = InstancePath(inst[0], (ISetupHelper)configuration,"Microsoft.VisualStudio", version);
					if (rs != null)
						return rs;
				}
			}
			while (got > 0);
			throw new Exception($"Visual Studio v{version} not found in setup COM");
		}

		private string TryGetVSPath(int majorVersion, int minorVersion)
		{
			string version = majorVersion + ".";
			if (minorVersion != -1)
				version += minorVersion;
			if (majorVersion >= 15)
				return TryGetVSSetupPath(version);
			string installationPath = null;
			if (Environment.Is64BitOperatingSystem)
			{
				installationPath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\VisualStudio\\" + version + "\\",
					"InstallDir",
					null);
			}
			else
			{
				installationPath = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\VisualStudio\\" + version + "\\",
					"InstallDir",
					null);
			}
			if (installationPath == null)
				throw new Exception($"Visual Studio v{version} not found in registry");
			return installationPath;
		}

		private void RegisterToolSet(int major, int minor, string vsName, int vsMajorVersion, int vsMinorVersion)
		{
			bool requiresWindowsTargetPlatformVersion = vsMajorVersion >= 15;
			try
			{
				string path = TryGetVSPath(vsMajorVersion, vsMinorVersion);
				if (path == null)
					throw new Exception($"Visual Studio v{vsMajorVersion}.{vsMinorVersion} not found in registry");
				toolSet.Items.Add(new ToolsetVersion(major, minor, vsName, requiresWindowsTargetPlatformVersion, path));

				LogLine(vsName + " found in " + path);
			}
			catch (Exception)
			{
				LogLine(vsName+$" installation folder not found. Skipping. Toolset v{major}.{minor} will not be available");
			}
		}

		private void ProjectView_Load(object sender, EventArgs e)
        {
			ResizeFont(this.Controls, FontScaleFactor);
			toolsetLabel.Size = toolsetLabel.PreferredSize;
			toolSet.Left = toolsetLabel.Right + 4;

			RegisterToolSet(12, 0, "VS 2013", 12,0);
			RegisterToolSet(14, 0, "VS 2015", 14,0);
			RegisterToolSet(14, 1, "VS 2017", 15,-1);

			PersistentState.Restore();
			if (PersistentState.Toolset != null)
			{
				toolSet.SelectedIndex = toolSet.Items.IndexOf(PersistentState.Toolset);

				if (toolSet.SelectedIndex == -1)	//toolset no longer available
				{
					if (toolSet.Items.Count > 0)
					{
						toolSet.SelectedIndex = 0;
						PersistentState.Toolset = toolSet.SelectedItem.ToString();
					}
					else
						LogLine("No Installation of Visual Studio found");
				}
			}
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
			startVSTimer.Enabled = false;
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

		private Solution LoadSolution(File file, bool batchLoad=false)
        {
			BeginLogSession();
			startVSTimer.Enabled = false;

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
				item.SubItems.Add(solution.Primary?.ToString());
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

		public static string LogNextEvent(ref Solution currentSolution, EventLog.Notification n)
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

			ToolTip tooltip = new ToolTip()
			{
				ShowAlways = true
			};
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
						LinkLabel ldomain = new LinkLabel()
						{
							Top = top,

							Font = f,
							Left = 20,
							Text = recent.Domain + "/"
						};
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
						LinkLabel lrecent = new LinkLabel()
						{
							Top = top,
							//lrecent.Font;

							Font = f,
							Left = left,
							Text = recent.Name
						};
						lrecent.Width = lrecent.PreferredWidth;
						recentSolutions.Controls.Add(lrecent);
						tooltip.SetToolTip(lrecent, recent.File.FullName);
						top += lrecent.Height;
						lrecent.Click += (sender, item2) => LoadSolution(recent.File);

						recentItems.Add(hasDomain ? recent.Domain : null, lrecent);
					}

				}

				ToolStripItem item = new ToolStripMenuItem(recent.ToString())
				{
					AutoToolTip = true,
					ToolTipText = recent.File.FullName
				};
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


		private void ToolSet_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ProjectView_Shown(object sender, EventArgs e)
        {
			toolStripStatusLabel1.Spring = true;
			toolStripStatusLabel1.TextAlign = ContentAlignment.MiddleLeft;
			statusStrip.Items[0].Text = "Persistent state stored in " + PersistentState.StateFile.FullName;

            UpdateRecentAndPaths(true);

			string[] parameters = Environment.GetCommandLineArgs();
			if (parameters.Length > 1)
				LoadFromParameter(parameters[1]);
        }

		private void LoadFromParameter(string p)
		{
			Solution solution = LoadSolution(new File(p));
			if (solution != null)
			{
				File outPath = PersistentState.GetOutPathFor(solution.Source);
				if (outPath.DirectoryExists)
					BuildCurrentSolution(solution, outPath);
					
			}
		}

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
			Program.End();
        }

        private void BuildToolStripMenuItem_Click(object sender, EventArgs e)
        {
			if (shownSolution == null)
				return;
			File outPath = PersistentState.GetOutPathFor(shownSolution.Source);
            if (!outPath.DirectoryExists)
            {
				string solutionName = shownSolution.Name + ".sln";
				DirectoryInfo preferred = shownSolution.Source.Directory.CreateSubdirectory(Project.WorkSubDirectory);
				outPath = new File( Path.Combine(preferred.FullName,solutionName) );


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
			startVSTimer.Enabled = false;
			foreach (Solution s in loadedSolutions)
			{
				s.Clear();
			}
			Solution.FlushGlobalProjects();
		}


        private ToolsetVersion GetToolsetVersion()
        {
            return (ToolsetVersion)(toolSet.SelectedItem);
        }

        private string GetOSVersion()
        {
            Process myProcess = new Process();

            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.Arguments = "/C ver";
            myProcess.StartInfo.RedirectStandardError = true;
            myProcess.StartInfo.RedirectStandardOutput = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.CreateNoWindow = true;
            try
            {
                if (!myProcess.Start())
                    throw new Exception("Failed to start process");

                string output = myProcess.StandardOutput.ReadToEnd().Trim();
                string original = output;
                myProcess.WaitForExit();

                const string match0 = "[Version ";
                const string match1 = "]";
                int begin = output.IndexOf(match0);
                if (begin == -1)
                    throw new Exception("Unable to find '"+match0+"' in result '"+original+"'");
                output = output.Substring(begin + match0.Length);
                int end = output.IndexOf(match1);
                if (end == -1)
                    throw new Exception("Unable to find '" + match1 + "' in result '" + original + "'");
                output = output.Substring(0, end);


				string[] segs = output.Split('.');
				if (segs.Length == 4)
				{
					segs[3] = "0";	//non-0 would not be a plattform target according to VS
					output = string.Join(".", segs);
				}
				else if (segs.Length < 4)
				{
					for (int i = segs.Length; i < 4; i++)
						output += ".0";

				}
                return output;
            }
            catch (Exception e)
            {
                EventLog.Warn(null,null,"Error (cmd.exe /C ver): " + e);
                return "";
            }
        }

        private void BuildCurrentSolution(Solution solution, File outPath)
		{
            try
            {
                if (solution.Build(outPath, GetToolsetVersion(), GetOSVersion(), overwriteExistingVSUserConfigToolStripMenuItem.Checked))
                {
                    if (solution == shownSolution)
                    {
                        openGeneratedSolutionToolStripMenuItem.Enabled = true;
                        openGeneratedSolutionButton.Enabled = true;
                    }

                    RefreshListView(solution);
                }
            }
            catch (Exception e)
            {
                EventLog.Warn(null, null, e.ToString());
            }
        }

        private void BuildAtToolStripMenuItem_Click(object sender, EventArgs e)
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
                PersistentState.SetOutPathFor(solution.Source, new File(chooseDestination.FileName));
                BuildToolStripMenuItem_Click(sender, e);
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

		private void OpenGeneratedSolutionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Solution solution = shownSolution;
			OpenGeneratedSolution(solution);
		}





		private bool OpenGeneratedSolution(Solution solution)
		{
			if (solution == null)
				return false;
			File slnPath = PersistentState.GetOutPathFor(solution.Source);
			if (slnPath.IsEmpty)
			{
				LogLine("Error: Out location unknown for '"+solution+"'. Chances are, this solution has not been generated.");
				return false;
			}
			if (!slnPath.Exists)
			{
				LogLine("Error: '"+slnPath.FullName+"' does not exist");
				return false;
			}
			if (solution.visualStudioProcess == null || solution.visualStudioProcess.HasExited)
			{ 
				Process myProcess = new Process();
				ToolsetVersion vs = GetToolsetVersion();
				if (vs.Path != null)
				{
					myProcess.StartInfo.WorkingDirectory = vs.Path;
					myProcess.StartInfo.FileName = Path.Combine(myProcess.StartInfo.WorkingDirectory, "devenv.exe");
				}
				else
					myProcess.StartInfo.FileName = "devenv.exe"; //not the full application path
				myProcess.StartInfo.Arguments = "\""+slnPath.FullName+"\"";
                try
                {
                    if (!myProcess.Start())
                        throw new Exception("Failed to start process");
                    solution.visualStudioProcess = myProcess;
					return true;
                }
                catch (Exception e)
                {
                    LogLine("Error: " + e);
                }
			}
			return false;
		}

        private void OverwriteExistingVSUserConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            overwriteExistingVSUserConfigToolStripMenuItem.Checked = !overwriteExistingVSUserConfigToolStripMenuItem.Checked;
        }

		private void SolutionListBox_ItemCheck(object sender, ItemCheckEventArgs e)
		{

		}

		private void Generate(Solution solution)
		{
            BeginLogSession();
            try
            {
                File outPath = PersistentState.GetOutPathFor(solution.Source);
                if (!outPath.Exists)
                {
                    DirectoryInfo preferred = solution.Source.Directory.CreateSubdirectory(Project.WorkSubDirectory);
                    if (preferred != null)
                    {
                        string outName = Path.Combine(preferred.FullName, solution.Name + ".sln");
                        EventLog.Inform(solution, null, "Out path for '" + solution + "' not known. Defaulting to " + outName);
                        outPath = new File(outName);
                        PersistentState.SetOutPathFor(solution.Source, outPath);
                    }
                }

                if (outPath.DirectoryExists)
                {
					bool newRecent;
					solution.Reload(out newRecent); //refresh
                    solution.Build(outPath, GetToolsetVersion(), GetOSVersion(), overwriteExistingVSUserConfigToolStripMenuItem.Checked);
                    if (solution == shownSolution)
                        ShowSolution(solution);

                    RefreshListView(solution);
                }
                else
                    EventLog.Warn(solution, null, "Error: Cannot export '" + solution + "'. Out path is not known.");
            }
            catch (Exception e)
            {
                EventLog.Warn(solution, null, "Error: "+e);
            }
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


		private IEnumerable<Solution> GetSelectedSolutions()
		{
			for (int i = 1; i < loadedSolutionsView.Items.Count; i++)
			{
				if (loadedSolutionsView.Items[i].Checked)
				{
					Solution solution = loadedSolutions[i-1];
					yield return solution;
				}
			}
		}

		public void RebuildSelected()
		{
			FlushProjects();
			BeginLogSession();
			foreach (var s in GetSelectedSolutions())
				Generate(s);
			EndLogSession();
		}

		private void GenerateSelectedButton_Click(object sender, EventArgs e)
		{
			RebuildSelected();
		}


		int startVSTimerAt = 0;
		private void openSelectedButton_Click(object sender, EventArgs e)
		{
			if (!startVSTimer.Enabled)
			{
				startVSTimerAt = 0;
				startVSTimer.Interval = 1;	//start immediately
				startVSTimer.Enabled = true;
			}
		}

		private void unloadSelectedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			startVSTimer.Enabled = false;
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
				LoadSolution(new File(file), true);

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

        private void overwriteExistingVSUserConfigToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            overwriteExistingVSUserConfigToolStripMenuItem.Checked = !overwriteExistingVSUserConfigToolStripMenuItem.Checked;
        }

		private void generateMakefileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			HashSet<Project> projects = new HashSet<Project>();

			BeginLogSession();

			GenerateSelectedButton_Click(null, null);

			foreach (var s in GetSelectedSolutions())
			{
				s.ScanEmptySources();
				foreach (var p in s.Projects)
				{
					projects.Add(p);
				}
			}
			DependencyTree.Clear();
			foreach (var p in projects)
				p.RegisterDependencyNodes();
			DependencyTree.ParseDependencies();
			DependencyTree.GenerateMakefiles();
			EndLogSession();
		}

		private void startVSTimer_Tick(object sender, EventArgs e)
		{
			bool longInterval = false;
			if (startVSTimerAt >= loadedSolutions.Count)
			{
				startVSTimer.Enabled = false;
				return;
			}
			{
				if (loadedSolutionsView.Items[startVSTimerAt+1].Checked)
				{
					Solution solution = loadedSolutions[startVSTimerAt];
					if (OpenGeneratedSolution(solution))
						longInterval = true;
				}
			}
			startVSTimer.Interval = longInterval ? 5000 : 1;
			startVSTimerAt++;
		}

		private void buildSelectedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RebuildSelected();
			var sol = new BuildSolutions();
			ResizeFont(sol.Controls, FontScaleFactor);
			sol.Begin(GetSelectedSolutions(), GetToolsetVersion(), RebuildSelected);
		}
	}
}
