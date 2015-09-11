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
                //try {

                    string solutionName = "";
                    {
                        var xreader = new XmlTextReader(openSolutionDialog.FileName);
                        int slashAt = Math.Max(openSolutionDialog.FileName.LastIndexOf('/'), openSolutionDialog.FileName.LastIndexOf('\\'));
                        if (slashAt != -1)
                            solutionName = openSolutionDialog.FileName.Substring(slashAt + 1);
                        else
                            solutionName = openSolutionDialog.FileName;
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.Load(xreader);
                        XmlNodeList xprojects = xdoc.SelectNodes("solution/project");

                        foreach (XmlNode xproject in xprojects)
                        {
                            Project.Add(xproject, new FileInfo(openSolutionDialog.FileName));
                        }
                        Directory.SetCurrentDirectory(openSolutionDialog.FileName.Substring(0, slashAt));
                    xreader.Close();
                    }

                    Project p;
                    while ((p = Project.GetNextUnloaded()) != null)
                    {
                        if (!p.HasPath && !p.FillPath(new FileInfo(openSolutionDialog.FileName)))
                            continue;

                        string filename = p.SourcePath.FullName;
                        var xreader = new XmlTextReader(filename);
                        XmlDocument xdoc = new XmlDocument();
                        xdoc.Load(xreader);
                        XmlNode xproject = xdoc.SelectSingleNode("project");
                        p.Load(xproject);
                        xreader.Close();
                    }




                    TreeNode tsolution = solutionView.Nodes.Add(solutionName);
                    foreach (Project project in Project.All)
                    {
						string projectName = project.Name;
						string options = project.Type ?? "Unknown";
						if (project == Project.Primary)
							options += ", Primary";
						if (options.Length > 0)
							projectName += " ("+options+")";

                        TreeNode tproject = tsolution.Nodes.Add(projectName);
						foreach (var c in project.CloneSources)
							tproject.Nodes.Add("clones: " + c.Name);
						foreach (var r in project.References)
                        {
                            TreeNode treference = tproject.Nodes.Add("references: "+r.project.Name + (r.includePath ? " (include)" : ""));
                        }
                        foreach (var s in project.Sources)
                        {
                            TreeNode tsource = tproject.Nodes.Add("sources");
                            s.ScanFiles();
                            AddSourceFolder(tsource.Nodes.Add(s.root.name), s.root);
                        }
                    }



//                }
                //catch (Exception ex)
                //{
                //    MessageBox.Show(ex.ToString());

                //}
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
    }
}
