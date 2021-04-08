using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Projector
{

	/// <summary>
	/// Helper structure to provide properties of a given file path
	/// </summary>
	public struct File
	{
		private string fullPath;

		/// <summary>
		/// Constructs the file path from a given absolute or relative path
		/// </summary>
		/// <param name="path">Absolute or relative path to the file. May or may not exist. May be null</param>
		public File(string path)
		{
			this.fullPath = path != null ? new System.IO.FileInfo(path).FullName : null;
		}

		/// <summary>
		/// Constructs the file from a file info struct
		/// </summary>
		/// <param name="info">File info struct to read from. May be null</param>
		public File(System.IO.FileInfo info)
		{
			fullPath = info?.FullName;
		}

		/// <summary>
		/// Fetches the full path of the local file descriptor. May return null
		/// </summary>
		public string FullName
		{
			get
			{
				return fullPath;
			}
		}

		/// <summary>
		/// Checks if the local file descriptor identifies an existing file
		/// </summary>
		public bool Exists
		{
			get
			{
				return fullPath != null && System.IO.File.Exists(fullPath);
			}
		}

		/// <summary>
		/// Checks if the local file descriptor has been initialized with null
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return fullPath == null;
			}
		}

		/// <summary>
		/// Checks if the local file descriptor has been initialized with a non-null path
		/// </summary>
		public bool IsNotEmpty
		{
			get
			{
				return fullPath != null;
			}
		}

		/// <summary>
		/// Checks if the directory, containing the local file descriptor, exists.
		/// </summary>
		public bool DirectoryExists
		{
			get
			{
				return fullPath != null && System.IO.Directory.Exists(new System.IO.FileInfo(fullPath).Directory.FullName);
			}

		}

		/// <summary>
		/// Fetches the directory containing the local file. May return null
		/// </summary>
		public System.IO.DirectoryInfo Directory
		{
			get
			{
				return fullPath != null ? new System.IO.FileInfo(fullPath).Directory : null;
			}
		}

		/// <summary>
		/// Fetches the name of the directory containing the local file. May return an empty string. Never null
		/// </summary>
		public string DirectoryName
		{
			get
			{
				return fullPath != null ? new System.IO.FileInfo(fullPath).DirectoryName : "";
			}
		}

		/// <summary>
		/// Adds a file extension or other kind of appendix to a given file descriptor
		/// </summary>
		/// <param name="a">File descriptor to append to</param>
		/// <param name="ext">Appendix</param>
		/// <returns>Concatenated file descriptor</returns>
		public static File operator +(File a, string ext)
		{
			if (a == null || a.IsEmpty)
				throw new ArgumentException("File descriptor is not valid for appending: "+a);
			return new File(a.FullName + ext);
		}

		/// <summary>
		/// Queries the name of the locally identified file. May return null
		/// </summary>
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

		/// <summary>
		/// Queries the name of the locally identified file without extension. May return null
		/// </summary>
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

	/// <summary>
	/// Registry for project folders
	/// </summary>
	public static class PathRegistry
    {
        private static Dictionary<string, File> map;
		private static HashSet<string>	ignore = new HashSet<string>();

		/// <summary>
		/// Attempts to locate the path of a project file by name.
		/// If the project is currently not registered or ignored, a dialog will open to select the matching project file.
		/// On success the requested project will be registered to point to the user-supplied path.
		/// On abort, the project will be ignored until the program is restarted.
		/// Automatically saves the local registry if deemed necessary.
		/// </summary>
		/// <param name="name">Project name to lookup</param>
		/// <returns>Existing full path to the project file or unset File descriptor if none was chosen</returns>
        public static File LocateProject(string name)
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

		/// <summary>
		/// Retrieves the file descriptor for the file presistently storing the local project registry
		/// </summary>
        public static File StateFile
        {
            get
            {
                return new File(System.IO.Path.Combine(PersistentState.StateFile.Directory.FullName, "pathRegistry.txt"));
            }
        }

        
        private static void LoadMap()
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


        private static void SaveMap()
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

		/// <summary>
		/// Queries an enumerable of all project names known to the local registry
		/// </summary>
		/// <returns>Enumerable of all known project names. Never null</returns>
		public static IEnumerable<string> GetAllProjectNames()
		{
			LoadMap();
			return map.Keys;
		}

		/// <summary>
		/// Removes a project from the local registry.
		/// Automatically saves the local registry if deemed necessary.
		/// </summary>
		/// <param name="name">Project name to remove</param>
		public static void UnsetPathFor(string name)
		{
			if (map.Remove(name))
				SaveMap();
		}

		/// <summary>
		/// Completely flushes the local registry content and saves
		/// </summary>
		public static void Clear()
		{
			map.Clear();
			SaveMap();
		}
	}
}