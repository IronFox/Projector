using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Projector
{
    internal class PathRegistry
    {
        private static Dictionary<string, FileInfo> map;

        internal static FileInfo LocateProject(string name)
        {
            LoadMap();
            FileInfo info;
            if (map.TryGetValue(name, out info))
                return info;

            ProjectView view = (ProjectView)Application.OpenForms["ProjectView"];
            OpenFileDialog dialog = view.OpenDialog;
            dialog.Title = "Locate project '" + name + "'";
            dialog.FileName = name + ".project";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                info = new FileInfo(dialog.FileName);
                map.Add(name, info);
                SaveMap();
                return info;
            }
            map.Add(name, null);
            SaveMap();
            return null;
        }


        static void LoadMap()
        {
            if (map != null)
                return;
            map = new Dictionary<string, FileInfo>();
            try
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader("pathRegistry.txt"))
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
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("pathRegistry.txt"))
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