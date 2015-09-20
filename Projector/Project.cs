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
		Win32,
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
		/// Name of the targeted platform (e.g. 'Win32' or 'x64')
		/// </summary>
		public readonly Platform	Platform;
		/// <summary>
		/// Set true to indicate that this is a fully optimized, exporting build configuration
		/// </summary>
		public readonly bool IsRelease;


		public Configuration(string name, Platform platform, bool isRelease)
		{
			Name = name;
			Platform = platform;
			IsRelease = isRelease;
		}


		public override string ToString()
		{
			return Name+"|"+Platform;
		}
        
    }


	public static partial class Extensions
	{
	}

	/// <summary>
	/// Loaded project. Each .project file must be loaded only once
	/// </summary>
    internal class Project
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
        public static CodeGroup h = new CodeGroup() { name = "header", tag = "ClInclude" };
        public static CodeGroup c = new CodeGroup() { name = "C", tag = "ClCompile" };
        public static CodeGroup shader = new CodeGroup() { name = "Shader", tag = "None" };
        public static CodeGroup image = new CodeGroup() { name = "Image", tag = "Image" };
        public static CodeGroup resource = new CodeGroup() { name = "Resource", tag = "ResourceCompile" };



		/// <summary>
		/// Warnings generated during the last solution-loading phase
		/// </summary>
        public static List<Notification> Warnings = new List<Notification>();

		/// <summary>
		/// General notifications generated during the last solution-loading phase
		/// </summary>
		public static List<Notification> Messages = new List<Notification>();


		/// <summary>
		/// All supported source extensions. Entries must be lower-case
		/// </summary>
        public static Dictionary<string, CodeGroup> ExtensionMap = new Dictionary<string, CodeGroup>()
        {
            {".h", h },
            {".hpp", h },
            {".h++", h },
            {".hh", h },

            {".c", c },

            {".hlsl", shader },

            {".cpp", cpp },
            {".c++", cpp },
            {".cc", cpp },

            {".ico", image },
            { ".rc", resource },
        };

		/// <summary>
		/// Flushes all static data to prepare a new solution import
		/// </summary>
        internal static void FlushAll()
        {
			Primary = null;
			Warnings.Clear();
			Messages.Clear();
            map.Clear();
            list.Clear();
            unloaded.Clear();
        }

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
                public Dictionary<CodeGroup, List<FileInfo>> groups = new Dictionary<CodeGroup, List<FileInfo>>();
                public List<Folder> subFolders = new List<Folder>();


				public void WriteFiles(StreamWriter writer)
				{
					foreach (var pair in groups)
					{
						foreach (var file in pair.Value)
						{
							writer.Write("  <" + pair.Key.tag);
							writer.Write(" Include=\"");
							writer.Write(file.FullName);
							if (pair.Key.tag == "None")
							{
								writer.WriteLine("\">");
								writer.WriteLine("    <FileType>Document</FileType>");
								writer.WriteLine("  </" + pair.Key.tag + ">");
							}
							else
								writer.WriteLine("\" />");
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
			}

            public Folder root;


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
                        List<FileInfo> list;
                        if (!sub.groups.TryGetValue(grp, out list))
                        {
                            list = new List<FileInfo>();
                            sub.groups.Add(grp, list);
                        }
                        list.Add(f);
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
            public void ScanFiles()
            {
                if (root != null)
                    return;
                root = new Folder();
                if (path.Exists)
                {

                    ScanFiles(root, path, recursive);
                }
            }




			public void WriteProjectGroup(StreamWriter writer)
			{
				writer.WriteLine("<ItemGroup>");
				root.WriteFiles(writer);
				writer.WriteLine("</ItemGroup>");
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


        }

		/// <summary>
		/// Reference of a remote project
		/// </summary>
        public struct Reference
        {
            public Project project;
            public bool includePath;

        }

		public struct Command
		{
			public string originalExecutable;
			public FileInfo	locatedExecutable;
			public string[]	parameters;
		}

		/// <summary>
		/// Fetches a list of all currently loaded projects
		/// </summary>
        public static IEnumerable<Project> All { get { return list; } }
		/// <summary>
		/// Currently chosen primary project. Can be null, but should not with a loaded solution
		/// </summary>
		public static Project Primary { get; private set; }


		/// <summary>
		/// Retrieves all locally referenced projects. May be empty, but never null
		/// </summary>
        public IEnumerable<Reference> References { get { return references; } }
		/// <summary>
		/// Retrieves all local source directories. May be empty, but never null
		/// </summary>
        public IEnumerable<Source> Sources { get { return sources; } }
		/// <summary>
		/// Retrieves all command line instructions to be executed ahead of building the local project. May be empty, but never null
		/// </summary>
		public IEnumerable<Command> PreBuildCommands { get { return preBuildCommands; } }
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
		public string Name { get; private set; }
		/// <summary>
		/// Retrieves the .project file the local project was loaded from. May be null
		/// </summary>
		public FileInfo SourcePath { get; private set; }
		/// <summary>
		/// Checks whether or not the local project has a source path. Usually this is only false, if the local project has not yet been loaded
		/// </summary>
		public bool HasSource { get { return SourcePath != null && SourcePath.Exists; } }


		private static Dictionary<string, Project> map = new Dictionary<string, Project>();
        private static List<Project> list = new List<Project>();
        private static Queue<Project> unloaded = new Queue<Project>();
        //private XmlNode xproject;

		List<FileInfo> customManifests = new List<FileInfo>();
		List<Command> preBuildCommands = new List<Command>();
        List<Source> sources = new List<Source>();
        Dictionary<string, string> macros = new Dictionary<string, string>();
        List<Reference> references = new List<Reference>();
		int customStackSize = -1;
        int roundTrip = 0;

		private List<Project>	cloneSources = new List<Project>();


		/// <summary>
		/// Attempts to automatically determine the local project source file. Paths next to the specified solution will be queried,
		/// as well as the global path registry.
		/// </summary>
		/// <param name="relativeToSolutionFile"></param>
		/// <returns></returns>
        public bool AutoConfigureSourcePath(FileInfo relativeToSolutionFile)
        {
            if (HasSource)
                return true;
            SourcePath = GetRelative(relativeToSolutionFile.Directory, Name + ".project");
            if (SourcePath.Exists)
                return true;
            SourcePath = PathRegistry.LocateProject(Name);
            return HasSource;
        }


		/// <summary>
		/// Retrieves a Guid from the source path of the local project.
		/// </summary>
        public Guid LocalGuid
        {
            get
            {
				return (SourcePath != null ? SourcePath.FullName : "").ToGuid();
            }
        }

		/// <summary>
		/// Sub-directory relative to the .project and .solution files that work-items are put into
		/// </summary>
		public static readonly string WorkSubDirectory = ".projector";


		/// <summary>
		/// Retrieves the Visual Studio project output file (including extension)
		/// </summary>
        public FileInfo OutFile
        {
            get
            {
                return new FileInfo(Path.Combine(Path.Combine(SourcePath.Directory.FullName, Path.Combine(WorkSubDirectory, "Projects" , Name)), Name + ".vcxproj"));
            }
        }

		/// <summary>
		/// Constructs the local project
		/// </summary>
		/// <param name="toolSetVersion">Active toolset version to use</param>
		/// <param name="configurations"></param>
		/// <returns></returns>
        public Tuple<FileInfo,Guid> SaveAs(int toolSetVersion, IEnumerable<Configuration> configurations)
        {
            FileInfo file = OutFile;
            if (!file.Directory.Exists)
                Directory.CreateDirectory(file.Directory.FullName);
            Guid id = LocalGuid;
            using (StreamWriter writer = File.CreateText(file.FullName))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

                writer.WriteLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"" + toolSetVersion + ".0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                writer.WriteLine("<ItemGroup Label=\"ProjectConfigurations\">");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<ProjectConfiguration Include=\"" + config + "\">");
                    writer.WriteLine("  <Configuration>" + config.Name + "</Configuration>");
                    writer.WriteLine("  <Platform>" + config.Platform + "</Platform>");
                    writer.WriteLine("</ProjectConfiguration>");
                }
                writer.WriteLine("</ItemGroup>");
                writer.WriteLine("<PropertyGroup Label=\"Globals\">");
                writer.WriteLine("<ProjectGuid>{" + id + "}</ProjectGuid>");
                writer.WriteLine("<Keyword>Win32Proj</Keyword>");
                writer.WriteLine("<RootNamespace>client</RootNamespace>");
                writer.WriteLine("</PropertyGroup>");
                writer.WriteLine("<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.Default.props\" />");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<PropertyGroup Condition=\"'$(Configuration)|$(Platform)' =='" + config + "'\" Label=\"Configuration\">");
                    writer.WriteLine("  <ConfigurationType>" + Type + "</ConfigurationType>");
                    writer.WriteLine("  <UseDebugLibraries>"+!config.IsRelease+"</UseDebugLibraries>");
                    writer.WriteLine("  <PlatformToolset>v" + toolSetVersion + "0</PlatformToolset>");
                    writer.WriteLine("  <WholeProgramOptimization>"+config.IsRelease+"</WholeProgramOptimization>");
                    writer.WriteLine("  <CharacterSet>Unicode</CharacterSet>");
                    writer.WriteLine("</PropertyGroup>");
                }
                writer.WriteLine("<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");
                writer.WriteLine("<ImportGroup Label=\"ExtensionSettings\">");
                writer.WriteLine("</ImportGroup>");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<ImportGroup Label=\"PropertySheets\" Condition=\"'$(Configuration)|$(Platform)'=='" + config + "'\">");
                    writer.WriteLine("  <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
                    writer.WriteLine("</ImportGroup>");
                }
                writer.WriteLine("<PropertyGroup Label=\"UserMacros\" />");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<PropertyGroup Condition=\"'$(Configuration)|$(Platform)' =='" + config + "'\">");
                    writer.WriteLine("  <LinkIncremental>" + !config.IsRelease + "</LinkIncremental>");
                    if (config.IsRelease)
					{ 
                        writer.WriteLine("  <OutDir>"+SourcePath.DirectoryName+Path.DirectorySeparatorChar+"</OutDir>");
						//if (config.platform == "Win32")
						writer.WriteLine("  <TargetName>$(ProjectName) "+config.Platform+"</TargetName>");
					}


                    string include = "";
                    foreach (var r in references)
                    {
                        if (!r.includePath)
                            continue;
                        foreach (var source in r.project.sources)
                        {

                            include += source.path.FullName;
                            include += ";";
                        }
                    }
                    writer.WriteLine("<IncludePath>" + include + "$(IncludePath)</IncludePath>");
                    //< LibraryPath > h:\testlib; D:\Program Files (x86)\OpenAL 1.1 SDK\libs\Win32;$(VC_LibraryPath_x86);$(WindowsSDK_LibraryPath_x86);$(NETFXKitsDir)Lib\um\x86 </ LibraryPath >
                    writer.WriteLine("</PropertyGroup>");
                }
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<ItemDefinitionGroup Condition=\"'$(Configuration)|$(Platform)' =='" + config + "'\">");
                    writer.WriteLine("  <ClCompile>");
                    writer.WriteLine("    <PrecompiledHeader>NotUsing</PrecompiledHeader>");
                    writer.WriteLine("    <WarningLevel>Level3</WarningLevel>");
                    writer.WriteLine("    <Optimization>" + (config.IsRelease ? "MaxSpeed" : "Disabled") + "</Optimization>");
                    if (config.IsRelease)
                    {
                        writer.WriteLine("    <FunctionLevelLinking>true</FunctionLevelLinking>");
                        writer.WriteLine("    <IntrinsicFunctions>true</IntrinsicFunctions>");
                    }

                    writer.Write("    <PreprocessorDefinitions>");
                    foreach (var m in macros)
                    {
                        writer.Write(m.Key);
                        if (m.Value != null && m.Value.Length > 0)
                            writer.Write("=" + m.Value);
                        writer.Write(";");
                    }
                    if (!config.IsRelease)
                        writer.Write("_DEBUG;");
                    if (SubSystem != null)
                        writer.Write("_"+ SubSystem.ToUpper()+ ";");
                    writer.Write("WIN32;");
                    writer.Write("%(PreprocessorDefinitions);%(PreprocessorDefinitions)");
                    writer.WriteLine("</PreprocessorDefinitions>");
                    writer.WriteLine("    <SDLCheck>true</SDLCheck>");
                    writer.WriteLine("    <RuntimeLibrary>MultiThreaded" + (config.IsRelease ? "" : "Debug") + "</RuntimeLibrary>");
                    writer.WriteLine("    <EnableParallelCodeGeneration>true</EnableParallelCodeGeneration>");
                    writer.WriteLine("    <MultiProcessorCompilation>true</MultiProcessorCompilation>");
                    writer.WriteLine("    <MinimalRebuild>false</MinimalRebuild>");
                    writer.WriteLine("  </ClCompile>");
                    writer.WriteLine("  <Link>");
                    if (SubSystem != null)
                        writer.WriteLine("    <SubSystem>"+ SubSystem + "</SubSystem>");
                    writer.WriteLine("    <GenerateDebugInformation>" + !config.IsRelease + "</GenerateDebugInformation>");
                    if (CustomStackSize != -1)
                        writer.WriteLine("    <AdditionalOptions>/STACK:" + CustomStackSize + " %(AdditionalOptions)</AdditionalOptions>");
                    writer.WriteLine("  </Link>");
					if (customManifests.Count > 0)
                    {
                        writer.WriteLine("  <Manifest>");
						StringBuilder combined = new StringBuilder();
						foreach (var manifest in customManifests)
						combined.Append(" \"").Append(manifest.FullName).Append("\"");
                        writer.WriteLine("    <AdditionalManifestFiles>" +  combined+ "</AdditionalManifestFiles>");
                        writer.WriteLine("  </Manifest>");
                    }
                    if (preBuildCommands.Count > 0)
                    {
                        writer.WriteLine("  <PreBuildEvent>");
                        foreach (Command cmd in PreBuildCommands)
                        {
							if (cmd.locatedExecutable != null)
							{
								writer.Write("    <Command>\"");
								writer.Write(cmd.locatedExecutable.FullName);
								writer.Write("\"");
								foreach (string parameter in cmd.parameters)
								{ 
									writer.Write(" \"");
									writer.Write(parameter);
									writer.Write("\"");
								}
								writer.WriteLine("</Command>");
							}
							else
								writer.WriteLine("    <Command>../../" + cmd + "</Command>");
                        }
                        writer.WriteLine("  </PreBuildEvent>");
                    }

                    writer.WriteLine("</ItemDefinitionGroup>");
                }
                foreach (Source source in sources)
                {
					source.WriteProjectGroup(writer);
                }

                writer.WriteLine("<ItemGroup>");
                foreach (var r in references)
                {
                    if (r.project.SourcePath != null)
                    {
                        writer.WriteLine("<ProjectReference Include=\"" + r.project.OutFile.FullName + "\">");
                        writer.WriteLine("<Project>{" + r.project.LocalGuid + "}</Project>");
                        writer.WriteLine("</ProjectReference>");
                    }
                }
                writer.WriteLine("</ItemGroup>");

                writer.WriteLine("<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.targets\" />");
                writer.WriteLine("<ImportGroup Label=\"ExtensionTargets\">");
                writer.WriteLine("</ImportGroup>");
                writer.WriteLine("</Project>");
                writer.Close();
            }

            using (StreamWriter writer = File.CreateText(file.FullName+".filters"))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                writer.WriteLine("<Project ToolsVersion=\"" + toolSetVersion + ".0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

                writer.WriteLine("<ItemGroup>");
                    WriteFilterDeclaration(writer, "Files");
                    foreach (Source source in sources)
                    {
						source.root.WriteFilterDeclarations(writer, "Files",sources.Count != 1);
                    }
                writer.WriteLine("</ItemGroup>");


                foreach (Source source in sources)
                {
					source.WriteProjectFilterGroup(writer, "Files",sources.Count != 1);
                }

                writer.WriteLine("</Project>");
                writer.Close();

            }
            if (!File.Exists(file.FullName + ".user"))
                using (StreamWriter writer = File.CreateText(file.FullName + ".user"))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<Project ToolsVersion=\"" + toolSetVersion + ".0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

                    foreach (var config in configurations)
                    {
                        writer.WriteLine("  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == '" + config + "'\">");
                        writer.WriteLine("    <LocalDebuggerWorkingDirectory>"+SourcePath.DirectoryName+"</LocalDebuggerWorkingDirectory>");
                        writer.WriteLine("  </PropertyGroup>");
                    }

                    writer.WriteLine("</Project>");
                    writer.Close();
                }

            return new Tuple<FileInfo, Guid>(file, id);
        }


		/// <summary>
		/// Declares a new filter in the writer stream
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="path"></param>
        public static void WriteFilterDeclaration(StreamWriter writer, string path)
        {
            writer.WriteLine("  <Filter Include=\"" + path + "\">");
            writer.WriteLine("    <UniqueIdentifier>{" + Guid.NewGuid() + "}</UniqueIdentifier>");
            writer.WriteLine("  </Filter>");
        }




        private bool loaded = false;



        public void Load(XmlNode xproject)
        {
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
                Project p = Add(xClone, SourcePath, this);
				if (p == this)
				{
					Warn("Cannot clone self.");
					continue;

				}
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
                unloaded.Enqueue(this);
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
				HashSet<string> have = new HashSet<string>();
				foreach (var m in p.CustomManifests)
					if (!have.Contains(m.FullName))
					{ 
						have.Add(m.FullName);
						customManifests.Add(m);
					}
                preBuildCommands.AddRange(p.preBuildCommands);
                sources.AddRange(p.sources);
                foreach (var pair in p.macros)
                    macros.Add(pair.Key, pair.Value);
                references.AddRange(p.references);

                if (Type == null)
                    Type = p.Type;

            }
            loaded = true;
			if (clone.Count > 0)
				cloneSources = clone;

            XmlNode xStack = xproject.Attributes.GetNamedItem("stackSize");
            if (xStack != null)
                customStackSize = int.Parse( xStack.InnerText);


			XmlNodeList xmanifests = xproject.SelectNodes("manifest");
			foreach (XmlNode xmanifest in xmanifests)
			{ 
				FileInfo file = new FileInfo(xmanifest.InnerText);
				if (file.Exists)
					customManifests.Add(file);
				else
					Warn("Cannot find manifest file '"+xmanifest.InnerText+"' relative to current directory '"+Directory.GetCurrentDirectory()+"'");
			}


            XmlNodeList xsources = xproject.SelectNodes("source");
            foreach (XmlNode xsource in xsources)
            {
                AddSource(xsource);
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
							Inform("'" + param + "' -> '" + test.FullName + "'");
							param = test.FullName;
						}
					}
					elements.Add(param);
				}



				if (elements.Count == 0)
				{
					Warn("Empty command encountered");
					continue;
				}

				Command cmd = new Command(){originalExecutable = elements[0],parameters = elements.ToArray(1)};
				FileInfo file = TryFindExecutable(cmd.originalExecutable);
				if (file == null)
					file = TryFindExecutable(cmd.originalExecutable+".exe");
				if (file == null)
					file = TryFindExecutable(cmd.originalExecutable + ".bat");
				if (file != null)
					cmd.locatedExecutable = file;
				else
					Warn("Unable to locate executable '"+cmd.originalExecutable+"'");

                preBuildCommands.Add(cmd);
            }
            XmlNodeList xmacros = xproject.SelectNodes("macro");
            foreach (XmlNode xmacro in xmacros)
            {
                AddMacro(xmacro);
            }
            XmlNodeList xreferences = xproject.SelectNodes("referenceProject");
            foreach (XmlNode xreference in xreferences)
            {
                AddReference(xreference);
            }
        }

		private FileInfo TryFindExecutable(string executable)
		{
			FileInfo file = new FileInfo(executable);
			if (file.Exists)
				return file;

			string pathEnv = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
			string[] paths = pathEnv.Split(Path.PathSeparator);
			foreach (string path in paths)
			{
				file = new FileInfo(Path.Combine(path, executable));
				if (file.Exists)
					return file;
			}
			return null;
		}

		/// <summary>
		/// Dequeues the next not-loaded project from the queue
		/// </summary>
		/// <returns>Project to load, or null if the queue is empty</returns>
        public static Project GetNextToLoad()
        {
            if (unloaded.Count == 0)
                return null;
            return unloaded.Dequeue();
        }


		/// <summary>
		/// Determines the final path of fileName (which may be a fully qualified, even absolute path)
		/// </summary>
		/// <param name="searchScope"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
        public static FileInfo GetRelative(DirectoryInfo searchScope, string fileName)
        {
            string current = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(searchScope.FullName);
            FileInfo rs = new FileInfo(fileName);
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
        public static Project Add(XmlNode xproject, FileInfo searchScope, Project warningsGoTo)
        {
            XmlNode xname = xproject.Attributes.GetNamedItem("name");
            if (xname == null)
            {
                Warn(warningsGoTo, "'name' attribute not set while parsing project entry");
                return null;
            }
            string name = xname.Value;
            Project p;
            if (map.TryGetValue(name, out p))
			{ 
				if (warningsGoTo == null)
					p.PurelyImplicitlyLoaded = false;
                return p;
			}


            p = new Project();
            p.Name = name;
			p.PurelyImplicitlyLoaded = warningsGoTo != null;
            map.Add(name, p);
            list.Add(p);
            unloaded.Enqueue(p);


            XmlNode xpath = xproject.Attributes.GetNamedItem("path");
            if (xpath != null)
            {
                p.SourcePath = GetRelative(searchScope.Directory, xpath.Value);
				if (!p.SourcePath.Exists)
					p.SourcePath = GetRelative(searchScope.Directory, Path.Combine(xpath.Value,name+".project"));
				if (!p.SourcePath.Exists)
                {
                    Warn(p, "Explicit project path '" + xpath.Value + "' does not exist relative to '" + searchScope.FullName + "'");
                    p.SourcePath = null;
                }
            }



            XmlNode xprim = xproject.Attributes.GetNamedItem("primary");
            if (xprim != null && xprim.Value.ToLower() == "true")
			{ 
				if (Primary != null)
				{
					p.Warn("Overriding primary project (was "+Primary.Name+")");
				}
				Primary = p;
			}
            return p;
        }


        private static void Warn(Project p, string message)
        {
            Warnings.Add(new Notification(p, message));
        }

        private void Warn(string message)
        {
            Warn(this, message);
        }

		private static void Inform(Project p, string message)
		{
			Messages.Add(new Notification(p, message));
		}

		private void Inform(string message)
		{
			Inform(this, message);
		}

        private void AddMacro(XmlNode xmacro)
        {
            XmlNode xname = xmacro.Attributes.GetNamedItem("name");
            if (xname == null)
            {
                Warn("'name' attribute not set for macro");
                return;
            }
            if (macros.ContainsKey(xname.Value))
                Warn("Redefining macro '"+xname.Value+"' to '"+xmacro.Value+"'");
            macros.Add(xname.Value, xmacro.Value);
        }

        private void AddReference(XmlNode xreference)
        {
            XmlNode xinclude = xreference.Attributes.GetNamedItem("includePath");
            Reference re;
            re.includePath = xinclude != null ? xinclude.Value == "true" : false;
            re.project = Add(xreference, SourcePath,this);
            references.Add(re);
        }

        private void AddSource(XmlNode xsource)
        {
            
            XmlNode xPath = xsource.Attributes.GetNamedItem("path");
            if (xPath == null)
            {
                Warn("'path' attribute missing while parsing source entry");
                return;
            }
            Source s = new Source();

            s.path = GetRelativeDir(SourcePath.Directory, xPath.Value);
            if (!s.path.Exists)
            {
                Warn("Source path '" + xPath.Value + "' does not exist relative to '" + SourcePath.FullName + "'");
                return;
            }

            XmlNodeList xExcludes = xsource.SelectNodes("exclude");
            if (xExcludes != null && xExcludes.Count > 0)
            {
                s.exclude = new List<Source.Exclude>();
                foreach (XmlNode xExclude in xExcludes)
                {
                    
                    XmlNode xFind = xExclude.Attributes.GetNamedItem("find");
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
                                Warn("Unable to determine exclusion of type");
                        }

                    }
                }
            }
            sources.Add(s);
        }

        private Project()
        { 
			PurelyImplicitlyLoaded = true;
		}

        public class Notification
        {
            public string Message
            {
                get; private set;
            }
            public Project Project
            {
                get; private set;
            }

            public override string ToString()
            {
                return (Project!=null ? Project.Name : "<root>")+ ": " + Message;
            }

            public Notification(Project p, string message)
            {
                this.Project = p;
                this.Message = message;
            }
        }
    }
}