using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Projector
{

    class Configuration
    {
        public string name,
                       platform;

        public bool isRelease;
        
    }


    internal class Project
    {
        public class CodeGroup
        {
            public string name;
            public string tag;

            public override int GetHashCode()
            {
                return name.GetHashCode();
            }
        }

        public static CodeGroup cpp = new CodeGroup() { name = "C++", tag = "ClCompile" };
        public static CodeGroup h = new CodeGroup() { name = "header", tag = "ClInclude" };
        public static CodeGroup c = new CodeGroup() { name = "C", tag = "ClCompile" };
        public static CodeGroup shader = new CodeGroup() { name = "Shader", tag = "None" };
        public static CodeGroup image = new CodeGroup() { name = "Image", tag = "Image" };
        public static CodeGroup resource = new CodeGroup() { name = "Resource", tag = "ResourceCompile" };
        //        <ItemGroup>
        //  <ResourceCompile Include="client.rc" />
        //</ItemGroup>
        //<ItemGroup>
        //  <Image Include="client.ico" />
        //  <Image Include="small.ico" />
        //</ItemGroup>
        //<ItemGroup>
        //  <ProjectReference Include="..\..\..\include\mvc12_project\DeltaWorks\DeltaWorks.vcxproj">
        //    <Project>{17522f7e-5234-482c-a387-a29791691c36}</Project>
        //  </ProjectReference>
        //</ItemGroup>

        public static List<Warning> Warnings = new List<Warning>();


        public static Dictionary<string, CodeGroup> extensionMap = new Dictionary<string, CodeGroup>()
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

        internal static void FlushAll()
        {
			Warnings.Clear();
            map.Clear();
            list.Clear();
            unloaded.Clear();
        }

        public class Source
        {
            public DirectoryInfo path;
            public bool recursive = true;


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
                            return info.Name == parameter || Match(info.Parent);
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
            public List<Exclude> exclude = null;

            public class Folder
            {
                public string name;
                public Dictionary<CodeGroup, List<FileInfo>> groups = new Dictionary<CodeGroup, List<FileInfo>>();
                public List<Folder> subFolders = new List<Folder>();

            }

            public Folder root;


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
                    if (extensionMap.TryGetValue(f.Extension.ToLower(), out grp))
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
                            sub.subFolders.Add(subsub);
                            ScanFiles(subsub, d, true);
                        }
                    }


            }

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
        }

        public struct Reference
        {
            public Project project;
            public bool includePath;

        }



        public static IEnumerable<Project> All { get { return list; } }
        public IEnumerable<Reference> References { get { return references; } }
        public IEnumerable<Source> Sources { get { return sources; } }
        public IEnumerable<string> PreBuildCommands { get { return preBuildCommands; }  }
        public int CustomStackSize { get { return customStackSize; }}
		public IEnumerable<FileInfo> CustomManifests { get { return customManifests; } }
		public IEnumerable<KeyValuePair<string,string> > Macros { get { return macros; } }



		private static Dictionary<string, Project> map = new Dictionary<string, Project>();
        private static List<Project> list = new List<Project>();
        private static Queue<Project> unloaded = new Queue<Project>();
        //private XmlNode xproject;

		List<FileInfo> customManifests = new List<FileInfo>();
        List<string> preBuildCommands = new List<string>();
        List<Source> sources = new List<Source>();
        Dictionary<string, string> macros = new Dictionary<string, string>();
        List<Reference> references = new List<Reference>();
		int customStackSize = -1;
        int roundTrip = 0;

		private List<Project>	cloneSources = new List<Project>();
		public IEnumerable<Project> CloneSources { get { return cloneSources;} }

        public string Type { get; private set; }
        public string SubSystem { get; private set; }
        public string Name { get; private set; }
        public static Project Primary { get; private set; }
        public FileInfo SourcePath { get; private set; }

        public bool HasPath { get { return SourcePath != null && SourcePath.Length > 0; } }

        internal bool FillPath(FileInfo from)
        {
            if (HasPath)
                return true;

            SourcePath = GetRelative(from.Directory, Name + ".project");
            if (SourcePath.Exists)
                return true;
            SourcePath = PathRegistry.LocateProject(Name);
            return SourcePath != null;
        }

        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        private void WriteGroup(StreamWriter writer, Source source)
        {
            writer.WriteLine("<ItemGroup>");
            WriteGroupMembers(writer, source.root);
            writer.WriteLine("</ItemGroup>");
        }

        private void WriteGroupMembers(StreamWriter writer, Source.Folder folder)
        {
            foreach (var pair in folder.groups)
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
                        writer.WriteLine("  </"+ pair.Key.tag +">");
                    }
                    else
                        writer.WriteLine("\" />");
                }
            }
            foreach (var child in folder.subFolders)
                WriteGroupMembers(writer,child);
        }
        private void WriteGroupFilter(StreamWriter writer, Source source, string rootPath, bool includeRootName)
        {
            writer.WriteLine("<ItemGroup>");
            WriteGroupMemberFilters(writer, source.root,rootPath,includeRootName);
            writer.WriteLine("</ItemGroup>");
        }

        private void WriteGroupMemberFilters(StreamWriter writer, Source.Folder folder, string path, bool includeName = true)
        {
            if (includeName)
            {
                if (path.Length > 0)
                    path += "\\";
                path += folder.name;
            }
            foreach (var pair in folder.groups)
            {
                foreach (var file in pair.Value)
                {
                    writer.Write("  <"+pair.Key.tag);
                    writer.Write(" Include=\"");
                    writer.Write(file.FullName);
                    writer.WriteLine("\">");
                    writer.WriteLine("    <Filter>"+ path + "</Filter>");
                    writer.WriteLine("  </" + pair.Key.tag + ">");
                }
            }
            foreach (var child in folder.subFolders)
                WriteGroupMemberFilters(writer, child, path);
        }

        private static void SwapBytes(byte[] guid, int left, int right)
        {
            byte temp = guid[left];
            guid[left] = guid[right];
            guid[right] = temp;
        }
        internal static void SwapByteOrder(byte[] guid)
        {
            SwapBytes(guid, 0, 3);
            SwapBytes(guid, 1, 2);
            SwapBytes(guid, 4, 5);
            SwapBytes(guid, 6, 7);
        }

        public Guid LocalGuid
        {
            get
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] hashed = sha.ComputeHash(GetBytes(SourcePath.FullName));
                byte[] newGuid = new byte[16];
                Array.Copy(hashed, 0, newGuid, 0, 16);

                // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
                newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));

                // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
                newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

                // convert the resulting UUID to local byte order (step 13)
                SwapByteOrder(newGuid);
                return new Guid(newGuid);
                //return new Guid(Array.subhashed);
            }
        }

        public FileInfo OutFile
        {
            get
            {
                
//                return new FileInfo(Path.Combine(SourcePath.Directory.FullName, Name + ".vcxproj"));
                return new FileInfo(Path.Combine(Path.Combine(SourcePath.Directory.FullName, Path.Combine(".projects", Name)), Name + ".vcxproj"));


            }
        }

        internal Tuple<FileInfo,Guid> SaveAs(string toolSet, IEnumerable<Configuration> configurations)
        {
            FileInfo file = OutFile;
            if (!file.Directory.Exists)
                Directory.CreateDirectory(file.Directory.FullName);
            Guid id = LocalGuid;
            using (StreamWriter writer = File.CreateText(file.FullName))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");

                writer.WriteLine("<Project DefaultTargets=\"Build\" ToolsVersion=\"" + toolSet + ".0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");
                writer.WriteLine("<ItemGroup Label=\"ProjectConfigurations\">");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<ProjectConfiguration Include=\"" + config.name + "|" + config.platform + "\">");
                    writer.WriteLine("  <Configuration>" + config.name + "</Configuration>");
                    writer.WriteLine("  <Platform>" + config.platform + "</Platform>");
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
                    writer.WriteLine("<PropertyGroup Condition=\"'$(Configuration)|$(Platform)' =='" + config.name + "|" + config.platform + "'\" Label=\"Configuration\">");
                    writer.WriteLine("  <ConfigurationType>" + Type + "</ConfigurationType>");
                    writer.WriteLine("  <UseDebugLibraries>"+!config.isRelease+"</UseDebugLibraries>");
                    writer.WriteLine("  <PlatformToolset>v" + toolSet + "0</PlatformToolset>");
                    writer.WriteLine("  <WholeProgramOptimization>"+config.isRelease+"</WholeProgramOptimization>");
                    writer.WriteLine("  <CharacterSet>Unicode</CharacterSet>");
                    writer.WriteLine("</PropertyGroup>");
                }
                writer.WriteLine("<Import Project=\"$(VCTargetsPath)\\Microsoft.Cpp.props\" />");
                writer.WriteLine("<ImportGroup Label=\"ExtensionSettings\">");
                writer.WriteLine("</ImportGroup>");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<ImportGroup Label=\"PropertySheets\" Condition=\"'$(Configuration)|$(Platform)'=='" + config.name + "|" + config.platform + "'\">");
                    writer.WriteLine("  <Import Project=\"$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props\" Condition=\"exists('$(UserRootDir)\\Microsoft.Cpp.$(Platform).user.props')\" Label=\"LocalAppDataPlatform\" />");
                    writer.WriteLine("</ImportGroup>");
                }
                writer.WriteLine("<PropertyGroup Label=\"UserMacros\" />");
                foreach (Configuration config in configurations)
                {
                    writer.WriteLine("<PropertyGroup Condition=\"'$(Configuration)|$(Platform)' =='" + config.name + "|" + config.platform + "'\">");
                    writer.WriteLine("  <LinkIncremental>" + !config.isRelease + "</LinkIncremental>");
                    if (config.isRelease)
					{ 
                        writer.WriteLine("  <OutDir>"+SourcePath.DirectoryName+"</OutDir>");
						//if (config.platform == "Win32")
						writer.WriteLine("  <TargetName>$(ProjectName) "+config.platform+"</TargetName>");
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
                    writer.WriteLine("<ItemDefinitionGroup Condition=\"'$(Configuration)|$(Platform)' =='" + config.name + "|" + config.platform + "'\">");
                    writer.WriteLine("  <ClCompile>");
                    writer.WriteLine("    <PrecompiledHeader>NotUsing</PrecompiledHeader>");
                    writer.WriteLine("    <WarningLevel>Level3</WarningLevel>");
                    writer.WriteLine("    <Optimization>" + (config.isRelease ? "MaxSpeed" : "Disabled") + "</Optimization>");
                    if (config.isRelease)
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
                    if (!config.isRelease)
                        writer.Write("_DEBUG;");
                    if (SubSystem != null)
                        writer.Write("_"+ SubSystem.ToUpper()+ ";");
                    writer.Write("WIN32;");
                    writer.Write("%(PreprocessorDefinitions);%(PreprocessorDefinitions)");
                    writer.WriteLine("</PreprocessorDefinitions>");
                    writer.WriteLine("    <SDLCheck>true</SDLCheck>");
                    writer.WriteLine("    <RuntimeLibrary>MultiThreaded" + (config.isRelease ? "" : "Debug") + "</RuntimeLibrary>");
                    writer.WriteLine("    <EnableParallelCodeGeneration>true</EnableParallelCodeGeneration>");
                    writer.WriteLine("    <MultiProcessorCompilation>true</MultiProcessorCompilation>");
                    writer.WriteLine("    <MinimalRebuild>false</MinimalRebuild>");
                    writer.WriteLine("  </ClCompile>");
                    writer.WriteLine("  <Link>");
                    if (SubSystem != null)
                        writer.WriteLine("  <SubSystem>"+ SubSystem + "</SubSystem>");
                    writer.WriteLine("    <GenerateDebugInformation>" + !config.isRelease + "</GenerateDebugInformation>");
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
                        foreach (string cmd in PreBuildCommands)
                        {
                            writer.WriteLine("    <Command>../../" + cmd + "</Command>");
                        }
                        writer.WriteLine("  </PreBuildEvent>");
                    }

                    writer.WriteLine("</ItemDefinitionGroup>");
                }
                foreach (Source source in sources)
                {
                    WriteGroup(writer, source);
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
                writer.WriteLine("<Project ToolsVersion=\"" + toolSet + ".0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

                writer.WriteLine("<ItemGroup>");
                    DeclareGroupFilter(writer, "Files");
                    foreach (Source source in sources)
                    {
                        DeclareGroupFilter(writer, source.root,"Files",sources.Count != 1);
                    }
                writer.WriteLine("</ItemGroup>");


                foreach (Source source in sources)
                {
                    WriteGroupFilter(writer, source, "Files",sources.Count != 1);
                }

                writer.WriteLine("</Project>");
                writer.Close();

            }
            if (!File.Exists(file.FullName + ".user"))
                using (StreamWriter writer = File.CreateText(file.FullName + ".user"))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("<Project ToolsVersion=\"" + toolSet + ".0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">");

                    foreach (var config in configurations)
                    {
                        writer.WriteLine("  <PropertyGroup Condition=\"'$(Configuration)|$(Platform)' == '" + config.name + "|" + config.platform + "'\">");
                        writer.WriteLine("    <LocalDebuggerWorkingDirectory>..\\..</LocalDebuggerWorkingDirectory>");
                        writer.WriteLine("  </PropertyGroup>");
                    }

                    writer.WriteLine("</Project>");
                    writer.Close();
                }

            return new Tuple<FileInfo, Guid>(file, id);
        }

        private void DeclareGroupFilter(StreamWriter writer, Source.Folder folder, string path, bool includeName=true)
        {
            if (includeName)
            {
                if (path.Length > 0)
                    path += "\\";
                path += folder.name;
            }

            DeclareGroupFilter(writer, path);
            foreach (var child in folder.subFolders)
                DeclareGroupFilter(writer, child,path);
        }

        private void DeclareGroupFilter(StreamWriter writer, string path)
        {
            writer.WriteLine("  <Filter Include=\"" + path + "\">");
            writer.WriteLine("    <UniqueIdentifier>{" + Guid.NewGuid() + "}</UniqueIdentifier>");
            writer.WriteLine("  </Filter>");
        }


        //public static Project Load(string projectName)
        //{
        //    string key = namespaceName + '/' + projectName;
        //    string path;
        //    if (!projectMap.TryGetValue(key, out path))
        //    {
        //        ProjectView view = (ProjectView)Application.OpenForms["ProjectView"];
        //        OpenFileDialog dialog = view.OpenDialog;
        //        if (dialog.ShowDialog() == DialogResult.OK)
        //        {

        //        }
        //    }
        //}

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
                preBuildCommands.Add(xcommand.InnerText);
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

        internal static Project GetNextUnloaded()
        {
            if (unloaded.Count == 0)
                return null;
            return unloaded.Dequeue();
        }



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
                return p;


            p = new Project();
            p.Name = name;
            map.Add(name, p);
            list.Add(p);
            unloaded.Enqueue(p);


            XmlNode xpath = xproject.Attributes.GetNamedItem("path");
            if (xpath != null)
            {
                p.SourcePath = GetRelative(searchScope.Directory, xpath.Value);
                if (!p.SourcePath.Exists)
                {
                    Warn(p, "Explicit project path '" + xpath.Value + "' does not exist relative to '" + searchScope.FullName + "'");
                    p.SourcePath = null;
                }
            }



            XmlNode xprim = xproject.Attributes.GetNamedItem("primary");
            if (xprim != null && xprim.Value.ToLower() == "true")
                Primary = p;
            return p;
        }


        private static void Warn(Project p, string message)
        {
            Warnings.Add(new Warning(p, message));
        }

        private void Warn(string message)
        {
            Warn(this, message);
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
        { }

        public class Warning
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

            public Warning(Project p, string message)
            {
                this.Project = p;
                this.Message = message;
            }
        }
    }
}