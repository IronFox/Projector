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

        public void LogLine(string line)
        {
            if (log.Text.Length > 0)
                log.Text += "\r\n";
            log.Text += line;
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
            toolSet.SelectedIndex = 0;
        }

        private string solutionName;

        private void LoadSolution(FileInfo file)
        {
            Project.FlushAll();

            log.Text = "Importing '" + file.FullName + "'...";

            PersistentState.MemorizeRecent(file);
            UpdateRecent();

            solutionName = "";
            {
                var xreader = new XmlTextReader(file.FullName);
                //int slashAt = Math.Max(file.FullName.LastIndexOf('/'), file.FullName.LastIndexOf('\\'));
                solutionName = file.Name.Substring(0,file.Name.Length - file.Extension.Length);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xreader);
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
            TreeNode tsolution = solutionView.Nodes.Add(solutionName);
            foreach (Project project in Project.All)
            {
                TreeNode tproject = tsolution.Nodes.Add(project.Name + (project == Project.Primary ? " (primary)" : "") + " " + project.Type);
                foreach (var r in project.References)
                {
                    TreeNode treference = tproject.Nodes.Add(r.project.Name + (r.includePath ? " (include)" : ""));
                }
                foreach (var s in project.Sources)
                {
                    TreeNode tsource = tproject.Nodes.Add("sources");
                    s.ScanFiles();
                    AddSourceFolder(tsource.Nodes.Add(s.root.name), s.root);
                }
            }

            foreach (var warning in Project.Warnings)
            {
                LogLine("Warning: " + warning);
            }
            if (Project.Warnings.Count == 0)
                LogLine("No issues");
            LogLine("Projects imported: " + Project.All.Count());
            solutionToolStripMenuItem.Enabled = Project.Primary != null;

        }


        private void UpdateRecent()
        {
            var collection = recentSolutionsToolStripMenuItem.DropDown.Items;
            collection.Clear();
            //while (collection[0].Text != "-")
            //{
            //    collection.RemoveAt(0);
            //}
            foreach (var recent in PersistentState.Recent)
            {
                ToolStripItem item = new ToolStripMenuItem(recent.Name);
                item.Click += (sender, item2) => LoadSolution(recent);
                collection.Add(item);

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
            UpdateRecent();

        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static string Relativate(DirectoryInfo dir, FileInfo file)
        {
            return new Uri(dir.FullName).MakeRelativeUri(new Uri(file.FullName)).ToString();
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo outPath = PersistentState.GetOutPathFor(solutionName);
            if (outPath == null || !outPath.Directory.Exists)
            {
                buildAtToolStripMenuItem_Click(sender, e);
                return;
            }
            DirectoryInfo dir = outPath.Directory;
            DirectoryInfo projectDir = Directory.CreateDirectory(Path.Combine(dir.FullName, ".projects"));
            List<Tuple<FileInfo, Guid, Project>> projects = new List<Tuple<FileInfo, Guid, Project>>();
            foreach (Project p in Project.All)
            {
                projects.Add(new Tuple<FileInfo, Guid,Project>(p.SaveAs(projectDir), Guid.NewGuid(), p));
            }
            StreamWriter writer = File.CreateText(outPath.FullName);

            string toolset = this.toolSet.SelectedText.Substring(0, this.toolSet.SelectedText.IndexOf(" ("));
            writer.WriteLine();
            writer.WriteLine("Microsoft Visual Studio Solution File, Format Version "+toolset+".00");
            writer.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
            Guid solutionGuid = Guid.NewGuid();
            foreach (var tuple in projects)
            {
                writer.WriteLine("Project(\"{" + solutionGuid + "}\") = \"" + tuple.Item3.Name + "\", \"" + Relativate(dir, tuple.Item1) + "\", \"{"
                    + tuple.Item2 + "}\");");
                writer.WriteLine("EndProject");
            }
            writer.WriteLine("Global");
            writer.WriteLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            writer.WriteLine("\t\tDebug | Win32 = Debug | Win32");
            writer.WriteLine("\t\tDebug | x64 = Debug | x64");
            writer.WriteLine("\t\tRelease | Win32 = Release | Win32");
            writer.WriteLine("\t\tRelease | x64 = Release | x64");
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
        }

        private void buildAtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string name = Project.Primary.Name;
            chooseDestination.Filter = "Solution | " + solutionName + ".sln";
            if (chooseDestination.ShowDialog() == DialogResult.OK)
            {
                PersistentState.SetOutPathFor(solutionName, new FileInfo(chooseDestination.FileName));
                buildToolStripMenuItem_Click(sender, e);
            }
        }
    }
}
