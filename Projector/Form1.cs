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

        private void LoadSolution(FileInfo file)
        {
            Project.FlushAll();

            log.Text = "Importing '" + file.FullName + "'...";

            PersistentState.MemorizeRecent(file);
            UpdateRecent();

            string solutionName = "";
            {
                var xreader = new XmlTextReader(file.FullName);
                //int slashAt = Math.Max(file.FullName.LastIndexOf('/'), file.FullName.LastIndexOf('\\'));
                solutionName = file.Name;
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
    }
}
