﻿using System;
using System.Collections.Concurrent;
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

namespace Projector
{
	public partial class BuildSolutions : Form
	{
		public BuildSolutions()
		{
			InitializeComponent();
		}

		private ToolsetVersion toolset;
		private static string msBuildPath = "";

		bool FindBuildPath(string current)
		{
			var dir = new DirectoryInfo(current);
			foreach (var d in dir.EnumerateDirectories())
			{
				if (FindBuildPath(d.FullName))
					return true;
			}

			foreach (var f in dir.EnumerateFiles())
			{
				if (f.Name.ToUpper() == "MSBUILD.EXE")
				{
					msBuildPath = f.DirectoryName;
					return true;
				}
			}
			return false;
		}


		public enum BuildMode
		{
			ForceRebuildAll,
			ForceRebuildFinalProjects,
			NoForceRebuilds
		}


		public class BuildJob : IComparable<BuildJob>
		{
			public readonly Platform Platform;
			public readonly Project Project;

			public readonly List<BuildJob> References = new List<BuildJob>();
			public readonly List<BuildJob> ReferencedBy = new List<BuildJob>();

			public bool IsCompiled { get; set; } = false;

			private ConsoleProcess process;

			public BuildJob(Platform platform, Project p)
			{
				Platform = platform;
				Project = p;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is BuildJob))
					return false;
				var other = (BuildJob)obj;
				return Platform == other.Platform && Project == other.Project;
			}

			public override int GetHashCode()
			{
				var hashCode = -326190272;
				hashCode = hashCode * -1521134295 + Platform.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<Project>.Default.GetHashCode(Project);
				return hashCode;
			}


			public void BeginMsBuild(string config, BuildMode mode, Action<string> logFunction)
			{
				bool rebuild = mode == BuildMode.ForceRebuildAll || (ReferencedBy.Count == 0 && mode == BuildMode.ForceRebuildFinalProjects);
				logFunction( (rebuild ? "Rebuilding " : "Building ") + this + "...");

				string parameters = "\"" + Project.OutFile + "\" /p:BuildProjectReferences=false /p:configuration=" + config;
				if (Platform != Platform.None)
					parameters += " /p:platform=" + Configuration.TranslateForVisualStudio(Platform);
				if (rebuild)
					parameters += " /t:Rebuild";
				Begin(parameters);
			}

			private void Begin(string parameters)
			{
				process = new ConsoleProcess(msBuildPath, "MSBuild.exe", parameters);
			}

			public override string ToString()
			{
				return Platform != Platform.None ? Project.Name + " " + Platform : Project.Name;
			}

			public bool IsRunning
			{
				get
				{
					return process != null && process.IsRunning;
				}
			}

			public ICollection<string> WaitForMsBuildExit()
			{
				process.WaitForExit();
				if (process.ExitCode != 0)
					return process.Output;
				return null;
			}

			internal void Kill()
			{
				if (process != null)
					process.KillTree();
			}

			internal bool LogErrors(BuildSolutions parent)
			{
				if (process != null && !process.IsRunning && process.ExitCode != 0)
				{
					parent.LogEvent("Build of " + this + " failed ("+process.ExitCode+"):");
					foreach (var line in process.Output)
						parent.LogEvent("    "+line);
					return true;
				}
				return false;
			}

			public int CompareTo(BuildJob other)
			{
				return ToString().CompareTo(other.ToString());
			}
		}



		List<BuildJob> projects = new List<BuildJob>();

		public void Begin(IEnumerable<Solution> loadedSolutions, ToolsetVersion toolset)
		{
			this.toolset = toolset;


			string[] segments = toolset.Path.Split(Path.DirectorySeparatorChar);
			if (FindBuildPath(Path.Combine(segments.Take(segments.Length - 2).ToArray())))
				LogEvent("Found MSBuild in " + msBuildPath);


			buildMode.SelectedIndex = 0;

			var rows = buildSelection.Items;
			
			HashSet<Project> added = new HashSet<Project>();
			foreach (var s in loadedSolutions)
			{
				foreach (var p in s.Projects)
				{
					if (p.ReferencedBy.Count > 0)
						continue;
					if (!added.Add(p))
						continue;

					if (IsPlatformAgnostic(p))
						projects.Add(new BuildJob(Platform.None, p));
					else
						foreach (Platform platform in Solution.GetTargetPlatforms())
							projects.Add(new BuildJob(platform, p));
				}
			}

			projects.Sort();

			selectionLocked = true;
			foreach (var p in projects)
			{
				var row = rows.Add(p.Project.Name);
				if (p.Platform != Platform.None)
					row.SubItems.Add(p.Platform.ToString());
				row.Checked = true;
			}

			selectionLocked = false;
			UpdateAllNoneCheckbox();

			buildConfigurations.Items.Clear();
			foreach (var cfg in Solution.GetPureBuildConfigurations())
			{
				buildConfigurations.Items.Add(cfg);
				if (cfg.Deploy)
					buildConfigurations.SelectedIndex = buildConfigurations.Items.Count - 1;
			}

			ShowDialog();
		}

		private void BuildSolutions_Shown(object sender, EventArgs e)
		{
			
		}


		private void UpdateAllNoneCheckbox()
		{
			bool anyFalse = false;
			bool anyTrue = false;
			for (int i = 1; i < buildSelection.Items.Count; i++)
				if (buildSelection.Items[i] != null && buildSelection.Items[i].Checked)
					anyTrue = true;
				else
					anyFalse = true;
			selectionLocked = true;
			buildSelection.Items[0].Checked = anyTrue && !anyFalse;
			selectionLocked = false;

		}

		bool selectionLocked = false;

		private void buildSelection_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (selectionLocked)
				return;
			int index = buildSelection.Items.IndexOf(e.Item);
			if (index == 0 && e.Item != null)
			{
				selectionLocked = true;
				for (int i = 1; i < buildSelection.Items.Count; i++)
				{
					var item = buildSelection.Items[i];
					if (item != null)
						item.Checked = e.Item.Checked;
				}
				selectionLocked = false;
			}
			else
				UpdateAllNoneCheckbox();
		}


		delegate void HandleLogEventCall(string text);

		internal void LogEvent(string input)
		{
			// InvokeRequired required compares the thread ID of the
			// calling thread to the thread ID of the creating thread.
			// If these threads are different, it returns true.
			if (this.eventLog.InvokeRequired)
			{
				HandleLogEventCall d = new HandleLogEventCall(LogEvent);
				this.Invoke(d, new object[] { input });
			}
			else
			{
				eventLog.Items.Add(input);
				eventLog.TopIndex = eventLog.Items.Count - 1;
			}
		}



		private static bool IsPlatformAgnostic(Project p)
		{
			if (p.Sources.Count != 0)
				return false;
			foreach (var r in p.References)
				if (!IsPlatformAgnostic(r.Project))
					return false;
			return true;
		}



		private BuildJob ScanDependencyGraph(Dictionary<BuildJob, BuildJob> outList, BuildJob build)
		{
			BuildJob existing;
			if (outList.TryGetValue(build,out existing))
				return existing;
			outList.Add(build, build);

			foreach (var p in build.Project.References)
			{
				var platform = IsPlatformAgnostic(p.Project) ? Platform.None : build.Platform;
				var dep = ScanDependencyGraph(outList, new BuildJob(platform, p.Project));
				build.References.Add( dep );
				dep.ReferencedBy.Add(build);
			}
			return build;
		}

		private void GetSelectedBuildTargets(Dictionary<BuildJob, BuildJob> outList)
		{
			for (int i = 1; i < buildSelection.Items.Count; i++)
			{
				if (buildSelection.Items[i].Checked)
				{
					ScanDependencyGraph(outList, projects[i - 1]);
				}
			}
		}
		public ICollection<BuildJob> GetSelectedBuildTargets()
		{
			var outList = new Dictionary<BuildJob, BuildJob>();
			GetSelectedBuildTargets(outList);
			return outList.Values;
		}

		List<BuildJob> activeJobs = new List<BuildJob>();

		int projectsBuilt = 0;

		private void buildButton_Click(object sender, EventArgs e)
		{
			if (activeJobs.Count > 0)
			{
				foreach (var job in activeJobs)
				{
					if (job.IsRunning)
					{
						LogEvent("Killing "+job+"...");
						job.Kill();
					}
				}
				foreach (var job in activeJobs)
				{
					LogEvent("Waiting for " + job + " resolution");
					job.WaitForMsBuildExit();
				}
				activeJobs.Clear();
				toolStripProgressBar.Value = 0;
				projectsBuilt = 0;
				toolStripProgressBar.Maximum = 1;

				LogEvent("Aborted all jobs");

				buildButton.Text = "Build";
				buildMode.Enabled = true;
				buildConfigurations.Enabled = true;

				return;
			}

			eventLog.Items.Clear();

			var workSet = GetSelectedBuildTargets();
			string config = buildConfigurations.SelectedItem.ToString();

			toolStripProgressBar.Value = 0;
			projectsBuilt = 0;
			toolStripProgressBar.Maximum = workSet.Count;




			foreach (var p in workSet)
			{
				if (p.References.Count == 0)
				{
					activeJobs.Add(p);
					p.BeginMsBuild(config, (BuildMode)buildMode.SelectedIndex, LogEvent);
				}
			}
			if (activeJobs.Count > 0)
			{
				buildButton.Text = "Abort Build";
				buildConfigurations.Enabled = false;
				buildMode.Enabled = false;
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			
			for (int i = 0; i < activeJobs.Count; i++)
			{
				var job = activeJobs[i];
				if (!job.IsRunning)
				{
					job.IsCompiled = true;
					activeJobs.RemoveAt(i);
					i--;
					projectsBuilt++;

					LogEvent("Finished building " + job);
					if (!job.LogErrors(this))
					{
						foreach (var c in job.ReferencedBy)
						{
							bool canBuild = true;
							foreach (var p in c.References)
								if (!p.IsCompiled)
								{
									canBuild = false;
									break;
								}
							if (canBuild)
							{
								activeJobs.Add(c);
								c.BeginMsBuild(buildConfigurations.SelectedItem.ToString(), (BuildMode)buildMode.SelectedIndex, LogEvent);
							}
						}
					}

					if (activeJobs.Count == 0)
					{
						buildButton.Text = "Build";
						buildConfigurations.Enabled = true;
						buildMode.Enabled = true;
					}
				}
			}

			
			toolStripProgressBar.Value = projectsBuilt;

			if (activeJobs.Count > 0)
			{
				toolStripStatusLabel.Text = "Building: "+string.Join(", ", activeJobs.Select(job => job.ToString()));
			}
			else
				toolStripStatusLabel.Text = "";
		}
	}
}
