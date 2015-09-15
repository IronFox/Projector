using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Projector
{
    public static partial class Extensions
    {
        public static string CoreName (this FileInfo info)
        {
            return info.Name.Substring(0, info.Name.IndexOf(info.Extension));
        }


    }


    internal class PathRegistry
    {
        private static Dictionary<string, FileInfo> map;
		private static HashSet<string>	ignore = new HashSet<string>();

        internal static FileInfo LocateProject(string name)
        {
            LoadMap();
			if (ignore.Contains(name))
				return null;
            FileInfo info;
            if (map.TryGetValue(name, out info))
			{ 
				if (info.Exists)
					return info;
			}
			MessageBox.Show("Project '"+name+"' is currently unknown. Please locate the .project file to continue...","Project not known");
            ProjectView view = (ProjectView)Application.OpenForms["ProjectView"];
            OpenFileDialog dialog = view.OpenDialog;
			do
			{
				dialog.Filter = "Project|"+name+".project";
				dialog.Title = "Locate project '" + name + "'";
				dialog.FileName = name + ".project";
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					dialog.Filter = "Projects|*.project|All files|*.*";
					info = new FileInfo(dialog.FileName);
					if (info.CoreName() != name)
					{
                        MessageBox.Show("The selected file's name does not match the expected project name '" + name + '"');
						continue;
					}
					map.Add(name, info);
					SaveMap();
					return info;
				}
				dialog.Filter = "Projects|*.project|All files|*.*";
				ignore.Add(name);
				return null;
			}
			while (true);
        }

        public static FileInfo StateFile
        {
            get
            {
                return new FileInfo(Path.Combine(PersistentState.StateFile.Directory.FullName, "pathRegistry.txt"));
            }
        }

        

        static void LoadMap()
        {
            if (map != null)
                return;
            map = new Dictionary<string, FileInfo>();
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(StateFile.FullName))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        string[] segs = line.Split('\t');
                        if (segs.Length == 2)
                        {
                            map.Add(segs[0], new FileInfo(segs[1]));
                        }

                    }
                }
            }
            catch
            { }
        }


        static void SaveMap()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(StateFile.FullName))
            {
                foreach (var entry in map)
                {
                    // If the line doesn't contain the word 'Second', write the line to the file. 
                    file.Write(entry.Key);
                    file.Write("\t");
                    file.WriteLine(entry.Value.FullName);
                }
            }



        }
    }
}