using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Projector
{



	/// <summary>
	/// Helper structure to provide properties of a given file path with non-null path
	/// </summary>
	public class FilePath
	{
		/// <summary>
		/// Full path of the local file descriptor. Never null
		/// </summary>
		public string FullName { get; }

		public string FullDirectoryName { get; }

		/// <summary>
		/// Constructs the file path from a given absolute or relative path
		/// </summary>
		/// <param name="path">Absolute or relative path to the file. May or may not exist. May be null</param>
		public FilePath(string path)
		{
			FullName = new System.IO.FileInfo(path).FullName;
			FullDirectoryName = Path.GetDirectoryName(FullName) ?? throw new ArgumentException("Given path '"+path+"' is not valid");
		}

		/// <summary>
		/// Constructs the file from a file info struct
		/// </summary>
		/// <param name="info">File info struct to read from</param>
		public FilePath(FileInfo info)
		{
			FullName = info.FullName;
			FullDirectoryName = Path.GetDirectoryName(FullName) ?? throw new ArgumentException("Given path '" + info + "' is not valid");
		}



		/// <summary>
		/// Checks if the local file descriptor identifies an existing file
		/// </summary>
		public bool Exists => File.Exists(FullName);


		/// <summary>
		/// Checks if the directory, containing the local file descriptor, exists.
		/// </summary>
		public bool DirectoryExists => System.IO.Directory.Exists(FullDirectoryName);

		/// <summary>
		/// Fetches the directory containing the local file. Never null
		/// </summary>
		public System.IO.DirectoryInfo Directory => new DirectoryInfo(FullDirectoryName);


		/// <summary>
		/// Adds a file extension or other kind of appendix to a given file descriptor
		/// </summary>
		/// <param name="a">File descriptor to append to</param>
		/// <param name="ext">Appendix</param>
		/// <returns>Concatenated file descriptor</returns>
		public static FilePath operator +(FilePath? a, string ext)
		{
			if (a is null)
				throw new ArgumentException("File descriptor is not valid for appending: " + a);
			return new FilePath(a.FullName + ext);
		}

		/// <summary>
		/// Queries the name of the locally identified file. May return null
		/// </summary>
		public string Name
		{
			get
			{
				return Path.GetFileName(FullName);
			}
		}

		/// <summary>
		/// Queries the name of the locally identified file without extension. May return null
		/// </summary>
		public string CoreName
		{
			get
			{
				return Path.GetFileNameWithoutExtension(FullName);
			}
		}
		public override int GetHashCode()
		{
			return FullName.GetHashCode();
		}
		public override bool Equals(object? obj)
		{
			return obj is FilePath fp && fp == this;
		}

		public override string ToString()
		{
			return FullName;
		}

		public static bool operator ==(FilePath a, FilePath b)
		{
			return a.FullName == b.FullName;
		}
		public static bool operator !=(FilePath a, FilePath b) => !(a == b);

		/// <summary>
		/// Resolves the given file-name relative to the local directory
		/// </summary>
		/// <param name="fileName">Relative or absolute file path</param>
		/// <returns>Descriptor for the resolved file name. Not required to exist</returns>
		public FilePath GetRelative(string fileName)
		{
			return new(Path.Combine(FullDirectoryName, fileName));
		}
	}


	/// <summary>
	/// Registry for project folders
	/// </summary>
	public static class PathRegistry
    {
        private static Dictionary<string, FilePath>? map;
		private static HashSet<string>	ignore = new HashSet<string>();

		/// <summary>
		/// Attempts to locate the path of a project file by name.
		/// If the project is currently not registered or ignored, a dialog will open to select the matching project file.
		/// On success the requested project will be registered to point to the user-supplied path.
		/// On abort, the project will be ignored until the program is restarted.
		/// Automatically saves the local registry if deemed necessary.
		/// </summary>
		/// <param name="name">Project name to lookup</param>
		/// <returns>Existing full path to the project file or null if none was chosen</returns>
        public static FilePath? LocateProject(string name)
        {
            var map = LoadMap();
			if (ignore.Contains(name))
				return null;
			FilePath? info;
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
					info = new FilePath(dialog.FileName);
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
				return null;
			}
			while (true);
        }

		/// <summary>
		/// Retrieves the file descriptor for the file presistently storing the local project registry
		/// </summary>
        public static FilePath StateFile
        {
            get
            {
                return new (Path.Combine(PersistentState.StateFile.FullDirectoryName, "pathRegistry.txt"));
            }
        }

        
        private static Dictionary<string, FilePath> LoadMap()
        {
            if (map is not null)
                return map;
            map = new Dictionary<string, FilePath>();
            try
            {
				if (StateFile.FullName is not null)
					using (System.IO.StreamReader file = new System.IO.StreamReader(StateFile.FullName))
					{
						string? line;
						while ((line = file.ReadLine()) is not null)
						{
							string[] segs = line.Split('\t');
							if (segs.Length == 2)
							{
								map.Add(segs[0], new FilePath(segs[1]));
							}

						}
					}
            }
            catch
            { }
			return map;
        }


        private static void SaveMap()
        {
			if (StateFile.FullName is not null && map is not null)
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
			return LoadMap().Keys;
		}

		/// <summary>
		/// Removes a project from the local registry.
		/// Automatically saves the local registry if deemed necessary.
		/// </summary>
		/// <param name="name">Project name to remove</param>
		public static void UnsetPathFor(string name)
		{
			if (map?.Remove(name) == true)
				SaveMap();
		}

		/// <summary>
		/// Completely flushes the local registry content and saves
		/// </summary>
		public static void Clear()
		{
			if (map is not null)
			{
				map.Clear();
				SaveMap();
			}
		}


	}
}