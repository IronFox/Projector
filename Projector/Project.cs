using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Projector
{

	/// <summary>
	/// Currently supported platforms.
	/// Since these are not loaded from files, an enumeration will do for now.
	/// </summary>
	public enum Platform
	{
		None,
		x86,
		x64,
		ARM
	}

	/// <summary>
	/// Build-configuration used during solution-building
	/// </summary>
    public class Configuration
    {
		/// <summary>
		/// Configuration name (e.g. 'Debug', or 'Release')
		/// </summary>
        public readonly string	Name;
		/// <summary>
		/// Macro that should be defined such that the code can recognize the locally used configuration. May be empty or null
		/// </summary>
		public readonly string	ConfigMacroIdentifier;
		/// <summary>
		/// Name of the targeted platform (e.g. 'Win32' or 'x64')
		/// </summary>
		public readonly Platform	Platform;
		/// <summary>
		/// Set true to indicate that this is a fully optimized, exporting build configuration
		/// </summary>
		public readonly bool IsRelease;
        /// <summary>
        /// Set true to export debug symbols
        /// </summary>
        public readonly bool Deploy;


		public Configuration(string name, string macroIdentifier, Platform platform, bool isRelease, bool deploy)
		{
			Name = name;
			ConfigMacroIdentifier = macroIdentifier;
			Platform = platform;
			IsRelease = isRelease;
            Deploy = deploy;
        }

		public static string TranslateForVisualStudio(Platform p)
		{
			return p == Platform.x86 ? "Win32" : p.ToString();
		}

		public static bool DefaultIncludePlatformInReleaseName(Platform p)
		{
			return p != Platform.x64;
		}

		public override string ToString()
		{
			return Name+"|"+TranslateForVisualStudio(Platform);
		}
        
    }

	/// <summary>
	/// Condition for certain operations declared by platform-name and config-name. If both are set, they are combined via and
	/// </summary>
	public struct Condition
	{
		/// <summary>
		/// Platform target to be matched. Platform.None if not enabled (always true)
		/// </summary>
		public readonly Platform IfPlatform;
		/// <summary>
		/// Configuration name target to be matched. null if not enabled (always true)
		/// </summary>
		public readonly string IfConfig;

		public Condition(Platform ifPlatform, string ifConfig)
		{
			IfPlatform = ifPlatform;
			IfConfig = ifConfig;
		}

		public Condition(XmlNode node, Solution domain, Project warnWhom)
		{
			XmlNode ifPlatform = node.Attributes.GetNamedItem("if_platform");
			try
			{
				if (ifPlatform != null && ifPlatform.Value.Length > 0)
					IfPlatform = (Platform)Enum.Parse(typeof(Platform),ifPlatform.Value);
				else
					IfPlatform = Platform.None;
			}
			catch
			{
				if (ifPlatform.Value == "x32")
					IfPlatform = Platform.x86;
				else
				{
					warnWhom.Warn(domain, "Unable to decode condition platform '" + ifPlatform.Value + "'. Supported values are ARM, x32, and x64");
					IfPlatform = Platform.None;
				}
			}
			XmlNode ifConfig = node.Attributes.GetNamedItem("if_config");
			if (ifConfig != null && ifConfig.Value.Length > 0)
				IfConfig = ifConfig.Value;
			else
				IfConfig = null;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Condition))
				return false;
			Condition other = (Condition)obj;
			return other == this;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			//if (IfPlatform != Pla)
			hash = hash * 31 + IfPlatform.GetHashCode();
			if (IfConfig != null)
				hash = hash * 31 + IfConfig.GetHashCode();
			return hash;
		}

		public static bool operator ==(Condition a, Condition b)
		{
			return a.IfPlatform == b.IfPlatform && a.IfConfig == b.IfConfig;
		}
		public static bool operator !=(Condition a, Condition b)
		{
			return a.IfPlatform != b.IfPlatform || a.IfConfig != b.IfConfig;
		}

		public override string ToString()
		{
			return "if (" + (IfPlatform != Platform.None ? IfPlatform.ToString(): "") + "," + (IfConfig ?? "") + ")";
		}

		public bool AlwaysTrue
		{
			get { return IfPlatform == Platform.None && IfConfig == null; }
		}

		public bool Excludes(Condition other)
		{
			bool differentA = IfPlatform != Platform.None && other.IfPlatform != Platform.None && IfPlatform != other.IfPlatform;
			bool differentB = IfConfig != null && other.IfConfig != null && IfConfig != other.IfConfig;
			return differentA || differentB;
		}

		public bool Test(Configuration config)
		{
			return (IfPlatform == Platform.None || IfPlatform == config.Platform)
					&&
					(IfConfig == null || IfConfig == config.Name);
		}

	}




	public static partial class Extensions
	{
	}

	/// <summary>
	/// Loaded project. Each .project file must be loaded only once
	/// </summary>
    public class Project
    {

		/// <summary>
		/// Details a category of possible source files (e.g. all .h files)
		/// </summary>
        public class CodeGroup
        {
            public string name;
            public string tag;

            public override int GetHashCode()
            {
                return name.GetHashCode();
            }
        }

		static Regex pathSplit = new Regex("(?:^| )(\"(?:[^\"]+|\"\")*\"|[^ ]*)", RegexOptions.Compiled);


        public static CodeGroup cpp = new CodeGroup() { name = "C++", tag = "ClCompile" };
        public static CodeGroup header = new CodeGroup() { name = "header", tag = "ClInclude" };
        public static CodeGroup c = new CodeGroup() { name = "C", tag = "ClCompile" };
        public static CodeGroup shader = new CodeGroup() { name = "Shader", tag = "None" };
        public static CodeGroup image = new CodeGroup() { name = "Image", tag = "Image" };
        public static CodeGroup resource = new CodeGroup() { name = "Resource", tag = "ResourceCompile" };
		public static CodeGroup cuda = new CodeGroup() { name = "CUDA", tag = "CudaCompile" };




		/// <summary>
		/// All supported source extensions. Entries must be lower-case
		/// </summary>
		public static Dictionary<string, CodeGroup> ExtensionMap = new Dictionary<string, CodeGroup>()
        {
            {".h", header },
            {".hpp", header },
            {".h++", header },
            {".hh", header },

            {".c", c },

            {".hlsl", shader },
			{".hlsli", shader },

			{".cu", cuda },


			{".cpp", cpp },
            {".c++", cpp },
            {".cc", cpp },

            {".ico", image },
            { ".rc", resource },
			{ ".rc2", resource },
		};


		/// <summary>
		/// Project source declaration. Each source targets a directory and may provide any number of exclusion-rules
		/// </summary>
		public class Source
        {
			/// <summary>
			/// Root directory to search from
			/// </summary>
            public DirectoryInfo path;
			/// <summary>
			/// Set true to recursively check sub-directories
			/// </summary>
            public bool recursive = true;


			/// <summary>
			/// Exclusion rule effective in the local source
			/// </summary>
            public class Exclude
            {
                public enum Type
                {
                    Find,
                    Dir,
                    File
                }

                public Type type = Type.Find;
                public string parameter;


                public bool Match(DirectoryInfo info)
                {
                    if (info == null)
                        return false;
                    switch (type)
                    {
                        case Type.Find:
                            return info.FullName.Contains(parameter);
                        case Type.Dir:
                            return info.Name == parameter;// || Match(info.Parent);
                    }
                    return false;
                }
                public bool Match(FileInfo info)
                {
                    if (info == null)
                        return false;
                    switch (type)
                    {
                        case Type.Find:
                            return info.FullName.Contains(parameter);
                        case Type.File:
                            return info.Name == parameter;
                    }
                    return false;
                }
            }
			/// <summary>
			/// List of all exclusion rules. May be null if there are none to be evaluated
			/// </summary>
            public List<Exclude> exclude = null;

			/// <summary>
			/// (possibly recursive) sub-directory that has been searched for possible files.
			/// Instantiated during ScanFiles()
			/// </summary>
            public class Folder
            {
                public string name;
                public Dictionary<CodeGroup, List<File>> groups = new Dictionary<CodeGroup, List<File>>();
                public List<Folder> subFolders = new List<Folder>();


				public void WriteFiles(XmlOut.Section writer)
				{
					foreach (var pair in groups)
					{
						foreach (var file in pair.Value)
							using (var fileSect = writer.SubSection(pair.Key.tag))
							{
								fileSect.AddParameter("Include", file.FullName);

								if (pair.Key.tag == "None")
									fileSect.SingleLine("FileType", "Document");
							}
					}
					foreach (var child in subFolders)
						child.WriteFiles(writer);
				}

				public void WriteFilters(StreamWriter writer, string path, bool includeName)
				{
					if (includeName)
					{
						if (path.Length > 0)
							path += "\\";
						path += name;
					}
					foreach (var pair in groups)
					{
						foreach (var file in pair.Value)
						{
							writer.Write("  <" + pair.Key.tag);
							writer.Write(" Include=\"");
							writer.Write(file.FullName);
							writer.WriteLine("\">");
							writer.WriteLine("    <Filter>" + path + "</Filter>");
							writer.WriteLine("  </" + pair.Key.tag + ">");
						}
					}
					foreach (var child in subFolders)
						child.WriteFilters(writer,path,true);
				}

				public void WriteFilterDeclarations(StreamWriter writer, string path, bool includeName)
				{
					if (includeName)
					{
						if (path.Length > 0)
							path += "\\";
						path += name;
					}

					WriteFilterDeclaration(writer, path);
					foreach (var child in subFolders)
						child.WriteFilterDeclarations(writer,path,true);

				}

				public IEnumerable<Tuple<CodeGroup, File>> EnumerateFiles()
				{
					foreach (var g in groups)
					{
						foreach (var f in g.Value)
							yield return new Tuple<CodeGroup, File>(g.Key, f);
					}
					foreach (var f in subFolders)
					{
						foreach (var rs in f.EnumerateFiles())
							yield return rs;
					}
				}
			}

            public Folder root;

			/// <summary>
			/// If set indicates that this source should be added to the include-directories of its own project as well as referencing external projects
			/// </summary>
			public bool includeDirectory = false;


			/// <summary>
			/// Checks whether or not a specific directory is excluded.
			/// If so, none of its contained files/sub-directories may be included either.
			/// </summary>
			/// <param name="dir"></param>
			/// <returns></returns>
			public bool IsExcluded(DirectoryInfo dir)
            {
                if (dir.Name.StartsWith("."))
                    return true;
                if (exclude != null)
                {
                    foreach (var ex in exclude)
                        if (ex.Match(dir))
                            return true;
                }
                return false;
            }
			/// <summary>
			/// Checks whether or not a specific file is excluded
			/// </summary>
			/// <param name="file"></param>
			/// <returns></returns>
            public bool IsExcluded(FileInfo file)
            {
                if (exclude != null)
                {
                    foreach (var ex in exclude)
                        if (ex.Match(file))
                            return true;
                }
                return false;
            }

            private void ScanFiles(Folder sub, DirectoryInfo current, bool recurse)
            {
                sub.name = current.Name;
                foreach (var f in current.EnumerateFiles())
                {
                    if (IsExcluded(f))
                        continue;

                    CodeGroup grp;
                    if (ExtensionMap.TryGetValue(f.Extension.ToLower(), out grp))
                    {
                        List<File> list;
                        if (!sub.groups.TryGetValue(grp, out list))
                        {
                            list = new List<File>();
                            sub.groups.Add(grp, list);
                        }
                        list.Add(new File(f));
                    }
                }

                if (recurse)
                    foreach (DirectoryInfo d in current.EnumerateDirectories())
                    {
                        if (!IsExcluded(d))
                        {
                            Folder subsub = new Folder();
							ScanFiles(subsub, d, true);
							if (subsub.groups.Count != 0 || subsub.subFolders.Count != 0)
								sub.subFolders.Add(subsub);
                        }
                    }


            }

			/// <summary>
			/// Scans for includable files. Will only search for files when called for the first time. Subsequent calls will have no effect
			/// </summary>
            public void ScanFiles(Solution domain, Project parent)
            {
                if (root != null)
                    return;

                root = new Folder();
                if (path.Exists)
                {
					EventLog.Inform(domain, parent,"Scanning "+path.Name+"...");
                    ScanFiles(root, path, recursive);
                }
            }




			public void WriteProjectGroup(XmlOut.Section parent)
			{
				using (var group = parent.SubSection("ItemGroup"))
					root.WriteFiles(group);
			}


			/// <summary>
			/// Writes the scanned local files into their respective filter groups
			/// </summary>
			/// <param name="writer">output writer stream</param>
			/// <param name="rootPath">Path that is put in front of any local filter groups</param>
			/// <param name="includeRootName">Set true to include the name of the local root folder, false to omit it</param>
		
			public void WriteProjectFilterGroup(StreamWriter writer, string rootPath, bool includeRootName)
			{
				writer.WriteLine("<ItemGroup>");
				root.WriteFilters(writer,rootPath,includeRootName);
				writer.WriteLine("</ItemGroup>");
			}

			public IEnumerable<Tuple<CodeGroup, File>> EnumerateFiles()
			{
				return root.EnumerateFiles();
			}
		}

		/// <summary>
		/// Reference of a remote project
		/// </summary>
        public struct Reference
        {
            public readonly Project Project;
			public readonly bool IncludePath;

			public Reference(Project project, bool includePath)
			{
				Project = project;
				IncludePath = includePath;
			}
        }

		public class LibraryInclusion
		{
			public string Name { get ; private set; }
			public DirectoryInfo Root { get; private set;}
			public List<Tuple<Condition, DirectoryInfo>> Includes { get; private set; }
			public List<Tuple<Condition, DirectoryInfo>> LinkDirectories { get; private set; }
			public List<Tuple<Condition, string>> Link { get; private set; }

            public override bool Equals(object obj)
			{
				LibraryInclusion other = obj as LibraryInclusion;
				if (other == null)
					return false;
                if (Link.Count == 0)
                    return other.Link.Count == 0 && Name == other.Name;
				if (Link.Count != other.Link.Count)
					return false;
				for (int i = 0; i < Link.Count; i++)
				{ 
					var a = Link[i];
					var b = other.Link[i];
					if (a.Item1 != b.Item1)
						return false;
					if (a.Item2 != b.Item2)
						return false;
				}
				return true;
			}

			public override int GetHashCode()
			{
				return Link.GetHashCode();
			}



			public LibraryInclusion(XmlNode xLib, Solution domain, Project warn)
			{
				XmlNode xWarn = xLib.Attributes.GetNamedItem("warn_if_not_found");
				bool doWarn = xWarn == null || xWarn.Value.ToUpper() != "FALSE";
				XmlNode xName = xLib.Attributes.GetNamedItem("name");
				if (xName == null)
				{
					warn.Warn(domain,"<includeLibrary> lacks 'name' attribute");
					Name = "<Unnamed Library>";
				}
				else
					Name = xName.Value;


                Includes = new List<Tuple<Condition, DirectoryInfo>>();
                LinkDirectories = new List<Tuple<Condition, DirectoryInfo>>();
                Link = new List<Tuple<Condition, string>>();


                HashSet<string>	knownRoots = new HashSet<string>();

				List<string>	missed = new List<string>();
				XmlNodeList xRootHints = xLib.SelectNodes("rootRegistryHint");
				foreach (XmlNode hint in xRootHints)
				{
					string fullKey = hint.InnerText;
					int lastSlash = fullKey.LastIndexOf('\\');
					if (lastSlash == -1)
					{
						warn.Warn(domain,"<includeLibrary>/<rootRegistryHint>: '"+fullKey+"' is not a valid registry value");
						continue;
					}
					string key = fullKey.Substring(0,lastSlash);
					string valueName = fullKey.Substring(lastSlash+1);

					object result = Registry.GetValue(key,valueName,null);
					if (result == null)
					{
						missed.Add(fullKey);
						continue;
					}
					DirectoryInfo info = new DirectoryInfo(result.ToString());
					if (!info.Exists)
					{
						warn.Warn(domain,Name+": The directory '"+result.ToString()+" does not exist");
					}
					else
					{
						if (Root == null)
							Root = info;
					}
				}
				if (Root == null && missed.Count == 0)
				{
					Root = domain.Source.Directory;
				}


				if (Root == null)
                {
					if (doWarn)
					{
						if (missed.Count == 1)
							warn.Warn(domain, Name + ": The given root location '" + missed[0] + "' could be evaluated. Is this library installed on your machine?");
						else
							warn.Warn(domain, Name + ": None of the " + missed.Count + " root locations could be evaluated. Is this library installed on your machine?");
					}
                }
                else
                {
                    XmlNodeList xIncludes = xLib.SelectNodes("include");
                    foreach (XmlNode xInclude in xIncludes)
                    {
                        DirectoryInfo dir = new DirectoryInfo(Path.Combine(Root.FullName, xInclude.InnerText));
                        if (dir.Exists)
							Includes.Add(new Tuple<Condition, DirectoryInfo>(new Condition(xInclude, domain, warn), dir));
                        else
                            warn.Warn(domain,Name + ": Declared include directory '" + xInclude.InnerText + "' does not exist");
                    }

                    XmlNodeList xLinkDirs = xLib.SelectNodes("linkDirectory");
                    foreach (XmlNode xLinkDir in xLinkDirs)
                    {
                        DirectoryInfo dir = new DirectoryInfo(Path.Combine(Root.FullName, xLinkDir.InnerText));
                        if (dir.Exists)
							LinkDirectories.Add(new Tuple<Condition, DirectoryInfo>(new Condition(xLinkDir, domain, warn), dir));
                        else
							warn.Warn(domain, Name + ": Declared link directory '" + xLinkDir.InnerText + "' does not exist relative to "+Root.FullName);
                    }
                }

                {
					XmlNodeList xLink = xLib.SelectNodes("link");
					foreach (XmlNode xl in xLink)
					{
						Condition condition = new Condition(xl, domain, warn);
						bool okayDoAdd = true;
						foreach (var dir in LinkDirectories)
						{
							if (condition.Excludes(dir.Item1))
								continue;

							FileInfo file = new FileInfo(Path.Combine(dir.Item2.FullName, xl.InnerText));
							if (!file.Exists)
							{
								okayDoAdd = false;
								warn.Warn(domain, Name + ": Declared library '" + xl.InnerText + "' could not be located in context '" + dir.Item2.FullName + "'. Skipping");
								break;
							}

						}
						if (okayDoAdd)
							Link.Add(new Tuple<Condition, string>(condition, xl.InnerText));
					}

				}

			}
			

		}


		public struct Command
		{
			public string		originalExecutable;
			public File			locatedExecutable;
			public string[]		parameters;

			public string QuotedParameters
			{
				get
				{
					if (parameters == null || parameters.Length == 0)
						return "";
					return " \"" + string.Join("\" \"", parameters) + "\"";
				}
			}

			public override string ToString()
			{
				return originalExecutable + " " + string.Join(" ", parameters);
			}
		}


		/// <summary>
		/// Fetches all conditioned build target names.
		/// </summary>
		public IEnumerable<KeyValuePair<Platform, string>> CustomTargetNames { get {  return customTargetNames; } }
		/// <summary>
		/// Retrieves all external libraries included by the local project
		/// </summary>
		public IEnumerable<LibraryInclusion> IncludedLibraries { get { return includedLibraries; } }
		/// <summary>
		/// Retrieves all locally referenced projects. May be empty, but never null
		/// </summary>
        public ICollection<Reference> References { get { return references; } }

		/// <summary>
		/// Options to add to the project configuration
		/// </summary>
		public string ExtraProjectOptions { get; private set; } = "";
		/// <summary>
		/// Retrieves disabled warning identifiers
		/// </summary>
		public IReadOnlyCollection<string> DisableWarnings => disableWarnings;

		public ICollection<Project> ReferencedBy { get { return referencedBy; } }
		/// <summary>
		/// Retrieves all local source directories. May be empty, but never null
		/// </summary>
        public ICollection<Source> Sources { get { return sources; } }
		/// <summary>
		/// Retrieves all included directories from any source (own sources, referenced projects, explicit inclusions). May be empty, but never null.
		/// Explicit inclusions are listed first, followed by local sources flagged as 'include', and finally referenced projects flagged as 'include'.
		/// </summary>
		public IEnumerable<string> IncludedPaths
		{
			get
			{
				foreach (var inc in explicitIncludePaths)
					yield return inc.FullName;
				foreach (var source in sources)
					if (source.includeDirectory)
						yield return source.path.FullName;
				foreach (var r in references)
				{
					if (!r.IncludePath)
						continue;
					foreach (var source in r.Project.sources)
					{
						if (source.includeDirectory)
							yield return source.path.FullName;
					}
				}
			}
		}
		/// <summary>
		/// Retrieves all command line instructions to be executed ahead of building the local project. May be empty, but never null
		/// </summary>
		public IReadOnlyList<Command> PreBuildCommands { get { return preBuildCommands; } }
		/// <summary>
		/// Any configuration custom stack size (in bytes) to be used for the local project. Negative if not used (-1)
		/// </summary>
        public int CustomStackSize { get { return customStackSize; }}
		/// <summary>
		/// Retrieves all custom manifest files to be included in the local project. May be empty, but never null
		/// </summary>
		public IEnumerable<FileInfo> CustomManifests { get { return customManifests; } }
		/// <summary>
		/// Retrieves all custom macros to be defined in the local project. May be empty, but never null
		/// </summary>
		public IEnumerable<KeyValuePair<string,string> > Macros { get { return macros; } }
		/// <summary>
		/// Details whether or not the local project was loaded as part of the solution (false), or by reference of some other project (true)
		/// </summary>
		public bool PurelyImplicitlyLoaded {  get; private set; }
		/// <summary>
		/// Retrieves all projects that the local project has cloned ahead of loading its own data. May be empty, but never null
		/// </summary>
		public IEnumerable<Project> CloneSources { get { return cloneSources; } }
		/// <summary>
		/// Retrieves the VS-specific type of the local project. Typical values are 'StaticLibrary', or 'Application'
		/// </summary>
		public string Type { get; private set; }
		/// <summary>
		/// Sub-type to be used with Type='Application'. Typical values are 'Windows', or 'Console'
		/// </summary>
		public string SubSystem { get; private set; }
		/// <summary>
		/// Name of the local project
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// Retrieves the .project file the local project was loaded from. May be null
		/// </summary>
		public File SourcePath { get; private set; }
		/// <summary>
		/// Checks whether or not the local project has a source path. Usually this is only false if the local project has not yet been loaded
		/// </summary>
		public bool HasSourceProject { get { return SourcePath.Exists; } }



        //private XmlNode xproject;

		List<LibraryInclusion> includedLibraries = new List<LibraryInclusion>();
		List<FileInfo> customManifests = new List<FileInfo>();
		List<Command> preBuildCommands = new List<Command>();
        List<Source> sources = new List<Source>();
        Dictionary<string, string> macros = new Dictionary<string, string>();
        List<Reference> references = new List<Reference>();
		private readonly HashSet<string> disableWarnings = new HashSet<string>();
		List<DirectoryInfo> explicitIncludePaths = new List<DirectoryInfo>();
		Dictionary<Platform,string>customTargetNames = new Dictionary<Platform,string>();
		HashSet<Project> referencedBy = new HashSet<Project>();
		int customStackSize = -1;
        int roundTrip = 0;

		private List<Project>	cloneSources = new List<Project>();


		/// <summary>
		/// Attempts to automatically determine the local project source file. Paths next to the specified solution will be queried,
		/// as well as the global path registry.
		/// </summary>
		/// <param name="relativeToSolutionFile"></param>
		/// <returns></returns>
        public bool AutoConfigureSourcePath(File relativeToSolutionFile)
        {
            if (HasSourceProject)
                return true;
            SourcePath = GetRelative(relativeToSolutionFile.Directory, Name + ".project");
            if (SourcePath.Exists)
                return true;
            SourcePath = PathRegistry.LocateProject(Name);
            return HasSourceProject;
        }


		/// <summary>
		/// Retrieves a Guid from the source path of the local project.
		/// </summary>
        public Guid LocalGuid
        {
            get
            {
				return (SourcePath.Exists ? SourcePath.FullName : "").ToGuid();
            }
        }

		/// <summary>
		/// Sub-directory relative to the .project and .solution files that work-items are put into
		/// </summary>
		public static readonly string WorkSubDirectory = ".projector";


		/// <summary>
		/// Retrieves the Visual Studio project output file (including extension)
		/// </summary>
        public File OutFile
        {
            get
            {
                return new File(Path.Combine(Path.Combine(SourcePath.Directory.FullName, Path.Combine(WorkSubDirectory, "Projects" , Name)), Name + ".vcxproj"));
            }
        }



		/// <summary>
		/// Constructs the local project
		/// </summary>
		/// <param name="toolSetVersion">Active toolset version to use</param>
		/// <param name="windowsTargetPlatformVersion">String containing the windows target platform version</param>
		/// <param name="configurations"></param>
		/// <returns></returns>
		public Tuple<File, Guid, bool> SaveAs(ToolsetVersion toolSetVersion, IEnumerable<Configuration> configurations, bool overwriteUserSettings, Solution domain)
        {
			File file = OutFile;
            if (!file.Directory.Exists)
                Directory.CreateDirectory(file.Directory.FullName);
            Guid id = LocalGuid;
			bool written = false;





			foreach (Source source in sources)
				source.ScanFiles(domain, this);

			bool haveCUDA = false;
			foreach (Source source in sources)
			{
				foreach (var f in source.EnumerateFiles())
				{
					if (f.Item1 == cuda)
					{
						haveCUDA = true;
						break;
					}
				}
				if (haveCUDA)
					break;
			}


			using (StreamWriter writer = new StreamWriter(new MemoryStream()))
			{
				writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
				writer.WriteLine("<Project ToolsVersion=\"" + toolSetVersion.OutXMLText + "\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

				writer.WriteLine("<ItemGroup>");
				WriteFilterDeclaration(writer, "Files");
				foreach (Source source in sources)
				{
					source.root.WriteFilterDeclarations(writer, "Files", sources.Count != 1);
				}
				writer.WriteLine("</ItemGroup>");


				foreach (Source source in sources)
				{
					source.WriteProjectFilterGroup(writer, "Files", sources.Count != 1);
				}

				writer.WriteLine("</Project>");
				if (Program.ExportToDisk(new File(file.FullName + ".filters"), writer))
					written = true;
			}

			if (!(file + ".user").Exists || overwriteUserSettings)
				using (StreamWriter writer = new StreamWriter(new MemoryStream()))
				{
					writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
					writer.WriteLine("<Project ToolsVersion=\"" + toolSetVersion.OutXMLText + "\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

					foreach (var config in configurations)
					{
						writer.WriteLine("  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == '" + config + "'\">");
						writer.WriteLine("    <LocalDebuggerWorkingDirectory>" + SourcePath.DirectoryName + "</LocalDebuggerWorkingDirectory>");
						writer.WriteLine("  </PropertyGroup>");
					}

					writer.WriteLine("</Project>");
					if (Program.ExportToDisk(file + ".user", writer))
						written = true;
				}

			using (StreamWriter streamWriter = new StreamWriter(new MemoryStream()))
			{
				using (XmlOut.Section rootWriter = new XmlOut.Section(streamWriter))
				{
					rootWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
					using (var projectWriter = rootWriter.SubSection("Project"))
					{
						projectWriter.AddParameter("DefaultTargets", "Build");
						projectWriter.AddParameter("ToolsVersion", toolSetVersion.OutXMLText);
						projectWriter.AddParameter("xmlns", "http://schemas.microsoft.com/developer/msbuild/2003");

						if (haveCUDA)
						{
							using (var group = projectWriter.SubSection("PropertyGroup"))
							using (var cudaProps = group.SingleLine("CUDAPropsPath"))
							{
								cudaProps.AddParameter("Condition", "'$(CUDAPropsPath)'==''");
								cudaProps.Write("$(VCTargetsPath)\\BuildCustomizations");
							}
						}
						using (var group = projectWriter.SubSection("ItemGroup"))
						{
							group.AddParameter("Label", "ProjectConfigurations");

							foreach (Configuration config in configurations)
							{
								using (var configSect = group.SubSection("ProjectConfiguration"))
								{
									configSect.AddParameter("Include", config.ToString());
									configSect.SingleLine("Configuration", config.Name);
									configSect.SingleLine("Platform", Configuration.TranslateForVisualStudio(config.Platform));
								}
							}
						}

						using (var group = projectWriter.SubSection("PropertyGroup"))
						{
							group.AddParameter("Label", "Globals");
							group.SingleLine("ProjectGuid", "{"+id+"}");
							group.SingleLine("ProjectName", Name);
							group.SingleLine("Keyword", Name);
							group.SingleLine("RootNamespace", Name);
							if (toolSetVersion.WindowsTargetPlatformVersion != null)
								group.SingleLine("WindowsTargetPlatformVersion", toolSetVersion.WindowsTargetPlatformVersion);
							if (haveCUDA)
								group.SingleLine("CudaToolkitCustomDir", "");
						}

						using (var group = projectWriter.SubSection("Import"))
						{
							group.AddParameter("Project", "$(VCTargetsPath)\\Microsoft.Cpp.Default.props");
						}

						foreach (Configuration config in configurations)
						{
							using (var group = projectWriter.SubSection("PropertyGroup"))
							{
								group.AddParameter("Condition", "'$(Configuration)|$(Platform)' =='" + config + "'");
								group.AddParameter("Label", "Configuration");

								group.SingleLine("ConfigurationType", Type);
								group.SingleLine("UseDebugLibraries", !config.IsRelease);
								group.SingleLine("PlatformToolset", "v" + (toolSetVersion.Major * 10 + toolSetVersion.Minor));
								group.SingleLine("WholeProgramOptimization", config.IsRelease);
								group.SingleLine("CharacterSet", "Unicode");
							}
						}

						using (var group = projectWriter.SubSection("Import"))
						{
							group.AddParameter("Project", "$(VCTargetsPath)\\Microsoft.Cpp.props");
						}
						using (var group = projectWriter.SubSection("ImportGroup"))
						{
							group.AddParameter("Label", "ExtensionSettings");
							if (haveCUDA)
							{
								using (var line = group.SingleLine("Import"))
									line.AddParameter("Project", "$(CUDAPropsPath)\\CUDA " + CUDA.Version + ".props");
							}

						}
						foreach (Configuration config in configurations)
						{
							using (var group = projectWriter.SubSection("ImportGroup"))
							{
								group.AddParameter("Label", "PropertySheets");
								group.AddParameter("Condition", "'$(Configuration)|$(Platform)'=='" + config + "'");
								using (var proj = group.SubSection("Import"))
								{
									proj.AddParameter("Project", "$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props");
									proj.AddParameter("Condition", "exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')");
									proj.AddParameter("Label", "LocalAppDataPlatform");
								}
							}
						}
						using (var group = projectWriter.SubSection("PropertyGroup"))
						{
							group.AddParameter("Label", "UserMacros");
						}

						foreach (Configuration config in configurations)
						{
							using (var group = projectWriter.SubSection("PropertyGroup"))
							{
								group.AddParameter("Condition", "'$(Configuration)|$(Platform)' == '" + config + "'");
								group.SingleLine("LinkIncremental", !config.IsRelease);

								if (config.Deploy)
								{
									group.SingleLine("OutDir", SourcePath.DirectoryName + Path.DirectorySeparatorChar);
								}
								{
									bool dummy;
									group.SingleLine("TargetName", GetReleaseTargetNameFor(config.Platform, out dummy));
								}


								List<string> includes, libPaths = new List<string>();

								includes = new List<string>(this.IncludedPaths);


								foreach (var lib in includedLibraries)
								{
									foreach (var inc in lib.Includes)
									{
										if (inc.Item1.Test(config))
											includes.Add(inc.Item2.FullName);
									}
									foreach (var linkDir in lib.LinkDirectories)
										if (linkDir.Item1.Test(config))
											libPaths.Add(linkDir.Item2.FullName);
								}

								if (includes.Count > 0)
									group.SingleLine("IncludePath", includes.Fuse(";") + ";$(IncludePath)");
								if (libPaths.Count > 0)
									group.SingleLine("LibraryPath", libPaths.Fuse(";") + ";$(LibraryPath)");
							}
						}



						foreach (Configuration config in configurations)
						{
							using (var group = projectWriter.SubSection("ItemDefinitionGroup"))
							{
								group.AddParameter("Condition", "'$(Configuration)|$(Platform)' =='" + config + "'");

								using (var clCompile = group.SubSection("ClCompile"))
								{
									clCompile.SingleLine("PrecompiledHeader", "NotUsing");
									clCompile.SingleLine("WarningLevel", "Level3");
									clCompile.SingleLine("Optimization", (config.IsRelease ? "MaxSpeed" : "Disabled"));

									if (config.IsRelease)
									{
										clCompile.SingleLine("FunctionLevelLinking", true);
										clCompile.SingleLine("IntrinsicFunctions", true);
									}

									using (var pred = clCompile.SingleLine("PreprocessorDefinitions"))
									{
										foreach (var m in macros)
										{
											pred.Write(m.Key);
											if (m.Value != null && m.Value.Length > 0)
												pred.Write("=" + m.Value);
											pred.Write(";");
										}
										if (!string.IsNullOrWhiteSpace(config.ConfigMacroIdentifier))
											pred.Write(config.ConfigMacroIdentifier + ";");
										pred.Write("PLATFORM_" + config.Platform.ToString().ToUpper() + ";");
										pred.Write("PLATFORM_TARGET_NAME_EXTENSION_STR=\"" + (Configuration.DefaultIncludePlatformInReleaseName(config.Platform) ? " " + config.Platform.ToString() : "") + "\";");
										if (SubSystem != null)
											pred.Write("_" + SubSystem.ToUpper() + ";");
										pred.Write("WIN32;");

										if (haveCUDA)
											pred.Write("_MBCS;");

										pred.Write("%(PreprocessorDefinitions)");
									}

									if (haveCUDA)
										using (var inc = clCompile.SingleLine("AdditionalIncludeDirectories"))
										{
											inc.Write("./;$(CudaToolkitDir)/include;");
											if (CUDA.CommonInc != null)
												inc.Write(CUDA.CommonInc.Replace('\\','/'));
										}

									clCompile.SingleLine("SDLCheck", true);
									clCompile.SingleLine("RuntimeLibrary", "MultiThreaded" + (config.IsRelease ? "" : "Debug"));
									clCompile.SingleLine("EnableParallelCodeGeneration", true);
									clCompile.SingleLine("MultiProcessorCompilation", true);
									clCompile.SingleLine("MinimalRebuild", false);


									if (disableWarnings.Count > 0)
										using (var dis = clCompile.SingleLine("DisableSpecificWarnings"))
										{
											dis.Write(string.Join(",", disableWarnings));
										}

									if (toolSetVersion.Major >= 14 && toolSetVersion.Minor >= 2)
										clCompile.SingleLine("AdditionalOptions", $"/Zc:__cplusplus {ExtraProjectOptions} %(AdditionalOptions)");
									else if (ExtraProjectOptions.Length > 0)
										clCompile.SingleLine("AdditionalOptions", $"{ExtraProjectOptions} %(AdditionalOptions)");
								}

								WriteLinkSection(group, config, haveCUDA);

								if (haveCUDA)
									WriteCUDASection(group, config);


								if (customManifests.Count > 0)
									using (var mani = group.SubSection("Manifest"))
									using (var additional = mani.SingleLine("AdditionalManifestFiles"))
									{
										StringBuilder combined = new StringBuilder();
										foreach (var manifest in customManifests)
										{
											additional.SeparatedWrite("\"", " ");
											additional.Write(manifest.FullName);
											additional.Write("\"");
										}
									}
								if (preBuildCommands.Count > 0)
									using (var pre = group.SubSection("PreBuildEvent"))
									{
										foreach (Command cmd in PreBuildCommands)
											using (var com = pre.SingleLine("Command"))
											{
												if (cmd.locatedExecutable.Exists)
												{
													com.Write("\"");
													com.Write(cmd.locatedExecutable.FullName);
													com.Write("\"");
													foreach (string parameter in cmd.parameters)
													{
														com.Write(" \"");
														com.Write(parameter);
														com.Write("\"");
													}
												}
												else
													com.Write("\"" + Path.Combine(new string[] { "..", "..", cmd.originalExecutable }) + "\"" + cmd.QuotedParameters);
											}
									}
							}
						}
						foreach (Source source in sources)
						{
							//source.ScanFiles(domain,this);
							source.WriteProjectGroup(projectWriter);
						}

						using (var group = projectWriter.SubSection("ItemGroup"))
						{
							foreach (var r in references)
							{
								if (r.Project.SourcePath.Exists)
									using (var projRef = group.SubSection("ProjectReference"))
									{
										projRef.AddParameter("Include", r.Project.OutFile.FullName);
										projRef.SingleLine("Project", "{" + r.Project.LocalGuid + "}");
									}
							}
						}

						using (var group = projectWriter.SubSection("Import"))
						{
							group.AddParameter("Project", "$(VCTargetsPath)\\Microsoft.Cpp.targets");
						}
						using (var group = projectWriter.SubSection("ImportGroup"))
						{
							group.AddParameter("Label", "ExtensionTargets");
							if (haveCUDA)
							{
								using (var imp = group.SubSection("Import"))
								{
									imp.AddParameter("Project", "$(CUDAPropsPath)\\CUDA "+CUDA.Version+".targets");
								}
							}
						}

					}
				}
				written = Program.ExportToDisk(file, streamWriter, written);
			}



			return new Tuple<File, Guid,bool>(file, id,written);
        }

		/// <summary>
		/// Writes the link section of the project into the specified parent section
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="config"></param>
		/// <param name="haveCUDA"></param>
		private void WriteLinkSection(XmlOut.Section parent, Configuration config, bool haveCUDA)
		{
			using (var linkSect = parent.SubSection("Link"))
			{
				if (SubSystem != null)
					linkSect.SingleLine("SubSystem", SubSystem);
				linkSect.SingleLine("GenerateDebugInformation", !config.Deploy);
				if (CustomStackSize != -1)
					linkSect.SingleLine("AdditionalOptions", $"/STACK:{CustomStackSize} %(AdditionalOptions)");

				List<string> libs = new List<string>();
				foreach (var lib in includedLibraries)
				{
					foreach (var link in lib.Link)
						if (link.Item1.Test(config))
							libs.Add(link.Item2);
				}
				if (haveCUDA)
				{
					libs.Add("cudart_static.lib");

					linkSect.SingleLine("AdditionalLibraryDirectories", "$(CudaToolkitLibDir)");
					linkSect.SingleLine("LinkTimeCodeGeneration", "UseLinkTimeCodeGeneration");
				}


				if (libs.Count > 0)
					linkSect.SingleLine("AdditionalDependencies", libs.Fuse(";") + ";%(AdditionalDependencies)");
			}
		}


		/// <summary>
		/// Writes the link-like CudaCompile section for the specified config
		/// </summary>
		/// <param name="writer">Out writer to put sections into</param>
		/// <param name="config">Configuration to read</param>
		private void WriteCUDASection(XmlOut.Section writer, Configuration config)
		{
			using (XmlOut.Section cudaCompile = writer.SubSection("CudaCompile"))
			{
				using (XmlOut.Line codeGeneration = cudaCompile.SingleLine("CodeGeneration"))
				{
					foreach (string version in CUDA.GpuCodes)
					{
						codeGeneration.SeparatedWrite($"compute_{version},sm_{version}");
					}
				}

				using (var line = cudaCompile.SingleLine("AdditionalOptions"))
				{
					line.Write("-Xcompiler \"/wd 4819\" --threads 0 -Wno-deprecated-gpu-targets %(AdditionalOptions)");
				}

				using (var line = cudaCompile.SingleLine("Include"))
				{
					line.SeparatedWrite("./");
					if (CUDA.CommonInc != null)
						line.SeparatedWrite(CUDA.CommonInc.Replace('\\', '/'));
				}

				using (var line = cudaCompile.SingleLine("Defines"))
				{
					line.SeparatedWrite("WIN32");
				}

				cudaCompile.SingleLine("Runtime", config.IsRelease ? "MT" : "MTd");
				cudaCompile.SingleLine("TargetMachinePlatform", config.Platform == Platform.x64 ? "64" : "32");
			}
		}

		public void RegisterDependencyNodes()
		{
			foreach (var s in sources)
			{
				foreach (Tuple<CodeGroup, File> f in s.EnumerateFiles())
				{
					DependencyTree.RegisterNode(this, f.Item2, f.Item1);
				}
			}
		}

		public string GetReleaseTargetNameFor(Platform platform, out bool isCustom)
		{
			string customTargetName;
			if (customTargetNames.TryGetValue(platform,out customTargetName))
			{ 
				isCustom = true;
				return customTargetName;
			}
			isCustom = false;
			return Configuration.DefaultIncludePlatformInReleaseName(platform) ? Name+" "+platform : Name;
		}


		/// <summary>
		/// Declares a new filter in the writer stream
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="path"></param>
        public static void WriteFilterDeclaration(StreamWriter writer, string path)
        {
            writer.WriteLine("  <Filter Include=\"" + path + "\">");
         //   writer.WriteLine("    <UniqueIdentifier>{" + Guid.NewGuid() + "}</UniqueIdentifier>");
            writer.WriteLine("  </Filter>");
        }




        private bool loaded = false;



        public void Load(XmlNode xproject, Solution domain)
        {
			ExtraProjectOptions = "";
			disableWarnings.Clear();

			XmlNode xType = xproject.Attributes.GetNamedItem("type");
            if (xType != null)
            {
                Type = xType.Value;
                int colon = Type.IndexOf(":");
                if (colon != -1)
                {
                    SubSystem = Type.Substring(colon + 1);
                    Type = Type.Substring(0, colon);
                }
            }
            XmlNodeList xClones = xproject.SelectNodes("clone");
            bool allThere = true;
            List<Project> clone = new List<Project>();
            foreach (XmlNode xClone in xClones)
            {
                Project p = AddProjectReference(xClone, SourcePath,domain, this,false);
				if (p == null)
					continue;
				if (p == this)
				{
					Warn(domain, "Cannot clone self.");
					continue;

				}
				p.AddReferencedBy(this);
                if (!p.loaded)
                {
                    allThere = false;
                }
                else
                    clone.Add(p);
            }
            if (!allThere)
            {
                roundTrip++;
				domain.EnqueueUnloaded(this);
                if (roundTrip > 2)
                {
                    throw new Exception("While loading project '"+Name+"': Self-cloning loop detected.");
                }
                return;
            }
            foreach (Project p in clone)
            {
                if (p.CustomStackSize != -1)
                    customStackSize = p.CustomStackSize;

				ExtraProjectOptions = p.ExtraProjectOptions;
				foreach (var warn in p.DisableWarnings)
					disableWarnings.Add(warn);

				HashSet<string> have = new HashSet<string>();
				foreach (var m in p.CustomManifests)
					if (!have.Contains(m.FullName))
					{ 
						have.Add(m.FullName);
						customManifests.Add(m);
					}
				foreach (var lib in p.IncludedLibraries)
					if (!includedLibraries.Contains(lib))
						includedLibraries.Add(lib);
                preBuildCommands.AddRange(p.preBuildCommands);
                sources.AddRange(p.sources);
				explicitIncludePaths.AddRange(p.explicitIncludePaths);
				foreach (var pair in p.macros)
                    macros.Add(pair.Key, pair.Value);
                references.AddRange(p.references);

				foreach (var c in p.CustomTargetNames)
					if (customTargetNames.ContainsKey(c.Key))
						customTargetNames[c.Key] = c.Value;
					else
						customTargetNames.Add(c.Key,c.Value);


                if (Type == null)
                    Type = p.Type;

            }
            loaded = true;
			if (clone.Count > 0)
				cloneSources = clone;

            XmlNode xStack = xproject.Attributes.GetNamedItem("stackSize");
            if (xStack != null)
                customStackSize = int.Parse( xStack.InnerText);


			XmlNode xExtraOptions = xproject.Attributes.GetNamedItem("extraProjectOptions");
			if (xExtraOptions != null)
				ExtraProjectOptions = xExtraOptions.InnerText ?? "";

			XmlNodeList xDisableWarnings = xproject.SelectNodes("disableWarning");
			if (xDisableWarnings != null)
			{
				foreach (XmlNode xDisable in xDisableWarnings)
				{
					XmlNode xCode = xDisable.Attributes.GetNamedItem("code");
					if (xCode != null && !string.IsNullOrWhiteSpace(xCode.InnerText))
						disableWarnings.Add(xCode.InnerText.Trim());
					else
						if (!string.IsNullOrWhiteSpace(xDisable.InnerText))
							disableWarnings.Add(xDisable.InnerText.Trim());
				}
			}



			XmlNodeList xmanifests = xproject.SelectNodes("manifest");
			foreach (XmlNode xmanifest in xmanifests)
			{ 
				FileInfo file = new FileInfo(xmanifest.InnerText);
				if (file.Exists)
					customManifests.Add(file);
				else
					Warn(domain, "Cannot find manifest file '" + xmanifest.InnerText + "' relative to current directory '" + Directory.GetCurrentDirectory() + "'");
			}


            XmlNodeList xsources = xproject.SelectNodes("source");
            foreach (XmlNode xsource in xsources)
            {
                AddSource(xsource, domain);
            }

			XmlNodeList xincludes = xproject.SelectNodes("include");
			foreach (XmlNode xinclude in xincludes)
			{
				AddInclude(xinclude, domain);
			}

			XmlNodeList xtargets = xproject.SelectNodes("targetName");
			foreach (XmlNode xtarget in xtargets)
			{
				XmlNode xplatform = xtarget.Attributes.GetNamedItem("platform");
				if (xplatform == null)
				{
					Warn(domain, "'platform' attribute not set for targetName setting. Supported platforms are 'ARM', 'x32', 'x64' Ignoring");
					continue;
				}
				Platform p;
				try
				{
					p = (Platform)Enum.Parse(typeof(Platform),xplatform.Value);
				}
				catch (ArgumentException)
				{
					Warn(domain, "'platform' attribute value '" + xplatform.Value + "' does not point to a supported platform. Supported platforms are 'ARM', 'x32', 'x64' Ignoring");
					continue;
				}

				if (customTargetNames.ContainsKey(p))
					customTargetNames[p] = xtarget.InnerText;
				else
					customTargetNames.Add(p,xtarget.InnerText);

			}

            XmlNodeList xcommands = xproject.SelectNodes("command");
            foreach (XmlNode xcommand in xcommands)
            {
				string source = xcommand.InnerText;

				//string source = "combined executable split parameter \"joined parameter\"";

				List<string> elements = new List<string>();
				foreach (Match match in pathSplit.Matches(source))
				{
					string param = match.Value.Trim(new char[]{' ','\"'});
					if (elements.Count > 0)
					{
						FileInfo test = new FileInfo(param);
						if (test.Exists)
						{ 
							//Inform("Parameter '"+param+"' of command '"+elements[0]+"' recognized as file. Replacing parameter with full path '"+test.FullName+"'");
							Inform(domain, "'" + param + "' -> '" + test.FullName + "'");
							param = test.FullName;
						}
					}
					elements.Add(param);
				}



				if (elements.Count == 0)
				{
					Warn(domain, "Empty command encountered");
					continue;
				}

				Command cmd = new Command(){originalExecutable = elements[0],parameters = elements.ToArray(1)};
				File file = TryFindExecutable(cmd.originalExecutable);
				if (!file.Exists)
					file = TryFindExecutable(cmd.originalExecutable+".exe");
				if (!file.Exists)
					file = TryFindExecutable(cmd.originalExecutable + ".bat");
				if (file.Exists)
					cmd.locatedExecutable = file;
				else
					Warn(domain, "Unable to locate executable '" + cmd.originalExecutable + "' relative to '"+ Directory.GetCurrentDirectory()+"'");

                preBuildCommands.Add(cmd);
            }

			XmlNodeList xLibraries = xproject.SelectNodes("includeLibrary");
			foreach (XmlNode xLib in xLibraries)
			{
				LibraryInclusion lib = new LibraryInclusion(xLib, domain, this);
				if (!includedLibraries.Contains(lib))
					includedLibraries.Add(lib);
			}


            XmlNodeList xmacros = xproject.SelectNodes("macro");
            foreach (XmlNode xmacro in xmacros)
            {
                AddMacro(xmacro,domain);
            }
            XmlNodeList xreferences = xproject.SelectNodes("referenceProject");
            foreach (XmlNode xreference in xreferences)
            {
                AddReference(xreference,domain);
            }
        }

		private File TryFindExecutable(string executable)
		{
			File file = new File(executable);
			if (file.Exists)
				return file;

			string pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
			string[] paths = pathEnv.Split(Path.PathSeparator);
			foreach (string path in paths)
			{
				file = new File(Path.Combine(path, executable));
				if (file.Exists)
					return file;
			}
			return new File();
		}




		/// <summary>
		/// Determines the final path of fileName (which may be a fully qualified, even absolute path)
		/// </summary>
		/// <param name="searchScope"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
        public static File GetRelative(DirectoryInfo searchScope, string fileName)
        {
            string current = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(searchScope.FullName);
			File rs = new File(fileName);
            Directory.SetCurrentDirectory(current);
            return rs;
        }
        public static DirectoryInfo GetRelativeDir(DirectoryInfo searchScope, string dirName)
        {
            string current = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(searchScope.FullName);
            DirectoryInfo rs = new DirectoryInfo(dirName);
            Directory.SetCurrentDirectory(current);
            return rs;
        }

		/// <summary>
		/// Adds or fetches a project via XML-entry
		/// </summary>
		/// <param name="xproject">Source project declaration</param>
		/// <param name="searchScope">Source path to look from when looking up relative paths</param>
		/// <param name="warningsGoTo">Project that is supposed to collect warnings. May be null</param>
		/// <returns>New or already existing project</returns>
        public static Project AddProjectReference(XmlNode xproject, File searchScope, Solution domain, Project warningsGoTo, bool listAsLocalProject)
        {
			//Debug.Assert(warningsGoTo == null || warningsGoTo.Solution == domain);
            XmlNode xname = xproject.Attributes.GetNamedItem("name");
            if (xname == null)
            {
				warningsGoTo.Warn(domain, "'name' attribute not set while parsing project entry");
                return null;
            }
            string name = xname.Value;
            Project p = domain.GetOrCreateProject(name, listAsLocalProject);
			if (warningsGoTo == null)
				p.PurelyImplicitlyLoaded = false;

            XmlNode xpath = xproject.Attributes.GetNamedItem("path");
            if (xpath != null)
            {
                p.SourcePath = GetRelative(searchScope.Directory, xpath.Value);
				if (!p.SourcePath.Exists)
					p.SourcePath = GetRelative(searchScope.Directory, Path.Combine(xpath.Value,name+".project"));
				if (!p.SourcePath.Exists)
                {
					p.Warn(domain, "Explicit project path '" + xpath.Value + "' does not exist relative to '" + searchScope.FullName + "'");
                    p.SourcePath = new File();
                }
            }



            XmlNode xprim = xproject.Attributes.GetNamedItem("primary");
            if (xprim != null && xprim.Value.ToLower() == "true")
			{ 
				domain.SetPrimary(p);
			}
            return p;
        }



        public void Warn(Solution domain, string message)
        {
			EventLog.Warn(domain, this, message);
        }


		private void Inform(Solution domain, string message)
		{
			EventLog.Inform(domain, this, message);
		}

        private void AddMacro(XmlNode xmacro, Solution domain)
        {
            XmlNode xname = xmacro.Attributes.GetNamedItem("name");
            if (xname == null)
            {
				Warn(domain, "'name' attribute not set for macro");
                return;
            }
            if (macros.ContainsKey(xname.Value))
				Warn(domain, "Redefining macro '" + xname.Value + "' to '" + xmacro.Value + "'");
            macros.Add(xname.Value, xmacro.InnerText);
        }

        private void AddReference(XmlNode xreference, Solution domain)
        {
            XmlNode xinclude = xreference.Attributes.GetNamedItem("includePath");
            if (xinclude == null)
                xinclude = xreference.Attributes.GetNamedItem("include");
            Reference re = new Reference(
									AddProjectReference(xreference, SourcePath,domain,this,true),
									xinclude != null ? xinclude.Value == "true" : true
									);
            references.Add(re);
			re.Project.AddReferencedBy(this);
        }

		private void AddReferencedBy(Project project)
		{
			referencedBy.Add(project);
		}

		private void AddSource(XmlNode xsource, Solution domain)
        {
            
            XmlNode xPath = xsource.Attributes.GetNamedItem("path");
            if (xPath == null)
            {
                Warn(domain, "'path' attribute missing while parsing source entry");
                return;
            }
            Source s = new Source();

			XmlNode xInclude = xsource.Attributes.GetNamedItem("include");
			if (xInclude != null)
			{
				string value = xInclude.Value;
				if (value.ToLower() == "true")
					s.includeDirectory = true;
				else
					if (value.ToLower() == "false")
					s.includeDirectory = false;
				else
					Warn(domain, "source.include expected to be either 'true' or 'false', but got '" + value + "'. Defaulting to " + s.includeDirectory);
			}

            s.path = GetRelativeDir(SourcePath.Directory, xPath.Value);
            if (!s.path.Exists)
            {
				Warn(domain, "Source path '" + xPath.Value + "' does not exist relative to '" + SourcePath.FullName + "'");
                return;
            }

            XmlNodeList xExcludes = xsource.SelectNodes("exclude");
            if (xExcludes != null && xExcludes.Count > 0)
            {
                s.exclude = new List<Source.Exclude>();
                foreach (XmlNode xExclude in xExcludes)
                {

					XmlNode xFind = xExclude.Attributes.GetNamedItem("find") ?? xExclude.Attributes.GetNamedItem("substr");
                    if (xFind != null)
                    {
                        var ex = new Source.Exclude();
                        ex.type = Source.Exclude.Type.Find;
                        ex.parameter = xFind.Value;
                        s.exclude.Add(ex);
                    }
                    else
                    {
                        XmlNode xDir = xExclude.Attributes.GetNamedItem("dir");
                        if (xDir == null)
                            xDir = xExclude.Attributes.GetNamedItem("directory");
                        if (xDir != null)
                        {
                            var ex = new Source.Exclude();
                            ex.type = Source.Exclude.Type.Dir;
                            ex.parameter = xDir.Value;
                            s.exclude.Add(ex);
                        }
                        else
                        {
                            XmlNode xFile = xExclude.Attributes.GetNamedItem("file");
                            if (xFile != null)
                            {
                                var ex = new Source.Exclude();
                                ex.type = Source.Exclude.Type.File;
                                ex.parameter = xFile.Value;
                                s.exclude.Add(ex);
                            }
                            else
								Warn(domain, "Unable to determine type of exclusion in source '" + xPath.Value + "' (supported types are 'find'/'substr', 'dir', and 'file')");
                        }

                    }
                }
            }
            sources.Add(s);
        }
		private void AddInclude(XmlNode xinc, Solution domain)
		{

			XmlNode xPath = xinc.Attributes.GetNamedItem("path");
			if (xPath == null)
			{
				Warn(domain, "'path' attribute missing while parsing include entry");
				return;
			}

			DirectoryInfo path = GetRelativeDir(SourcePath.Directory, xPath.Value);
			if (!path.Exists)
			{
				Warn(domain, "Include path '" + xPath.Value + "' does not exist relative to '" + SourcePath.FullName + "'");
				return;
			}
			explicitIncludePaths.Add(path);
		}

		public Project(string name)
        { 
			PurelyImplicitlyLoaded = true;
			Name = name;
		}


		public override string ToString()
		{
			return Name;
		}
    }
}