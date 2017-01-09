using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Projector
{


	public struct File
	{
		string fullPath;

		public File(string path)
		{
			this.fullPath = path != null ? new System.IO.FileInfo(path).FullName : null;
		}
		public File(System.IO.FileInfo info)
		{
			fullPath = info?.FullName;
		}

		public string FullName
		{
			get
			{
				return fullPath;
			}
		}

		public bool Exists
		{
			get
			{
				return fullPath != null && System.IO.File.Exists(fullPath);
			}
		}

		public bool IsEmpty
		{
			get
			{
				return fullPath == null;
			}
		}

		public bool IsNotEmpty
		{
			get
			{
				return fullPath != null;
			}
		}

		public bool DirectoryExists
		{
			get
			{
				return fullPath != null && System.IO.Directory.Exists(new System.IO.FileInfo(fullPath).Directory.FullName);
			}

		}

		public System.IO.DirectoryInfo Directory
		{
			get
			{
				return fullPath != null ? new System.IO.FileInfo(fullPath).Directory : null;
			}
		}

		public string DirectoryName
		{
			get
			{
				return fullPath != null ? new System.IO.FileInfo(fullPath).DirectoryName : "";
			}
		}

		public static File operator +(File a, string ext)
		{
			return new File(a.FullName + ext);
		}
		public string Name
		{
			get
			{
				if (fullPath == null)
					return null;
				var fi = new System.IO.FileInfo(fullPath);
				return fi.Name;
			}
		}

		public string CoreName
		{
			get
			{
				if (fullPath == null)
					return null;
				var fi = new System.IO.FileInfo(fullPath);
				return fi.Name.Substring(0, fi.Name.IndexOf(fi.Extension));
			}
		}
		public override int GetHashCode()
		{
			return fullPath.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			return obj is File && ((File)obj) == this;
		}

		public override string ToString()
		{
			return fullPath ?? "<null>";
		}

		public static bool operator ==(File a, File b)
		{
			return a.fullPath == b.fullPath;
		}
		public static bool operator !=(File a, File b)
		{
			return a.fullPath != b.fullPath;
		}
	}


	internal class PathRegistry
    {
        private static Dictionary<string, File> map;
		private static HashSet<string>	ignore = new HashSet<string>();

        internal static File LocateProject(string name)
        {
            LoadMap();
			if (ignore.Contains(name))
				return new File();
			File info;
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
					info = new File(dialog.FileName);
					if (info.CoreName != name)
					{
                        MessageBox.Show("The selected file's name does not match the expected project name '" + name + '"');
						continue;
					}
					map[name] = info;
					//map.Add(name, info);
					SaveMap();
					return info;
				}
				dialog.Filter = "Projects|*.project|All files|*.*";
				ignore.Add(name);
				return new File();
			}
			while (true);
        }

        public static File StateFile
        {
            get
            {
                return new File(System.IO.Path.Combine(PersistentState.StateFile.Directory.FullName, "pathRegistry.txt"));
            }
        }

        

        static void LoadMap()
        {
            if (map != null)
                return;
            map = new Dictionary<string, File>();
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
                            map.Add(segs[0], new File(segs[1]));
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

		internal static IEnumerable<string> GetAllProjectNames()
		{
			LoadMap();
			return map.Keys;
		}

		internal static void UnsetPathFor(string name)
		{
			if (map.Remove(name))
				SaveMap();
		}

		internal static void Clear()
		{
			map.Clear();
			SaveMap();
		}
	}
}