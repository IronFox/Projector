using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Projector
{
    internal class Project
    {
        public class CodeGroup
        {
            public string name;
            public bool compiles;

            public override int GetHashCode()
            {
                return name.GetHashCode();
            }
        }

        public static CodeGroup cpp = new CodeGroup() { name = "C++", compiles = true };
        public static CodeGroup h = new CodeGroup() { name = "header", compiles = false };
        public static CodeGroup c = new CodeGroup() { name = "C", compiles = true };

        public static List<Warning> Warnings = new List<Warning>();


        public static Dictionary<string, CodeGroup> extensionMap = new Dictionary<string, CodeGroup>()
        {
            {".h", h },
            {".hpp", h },
            {".h++", h },
            {".hh", h },

            {".c", c },

            {".cpp", cpp },
            {".c++", cpp },
            {".cc", cpp },
        };

        internal static void FlushAll()
        {
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
                    Dir
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
                            //                        case Type.Dir:
                            //                          return info.Name == parameter || Match(info.Parent);
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

        private static Dictionary<string, Project> map = new Dictionary<string, Project>();
        private static List<Project> list = new List<Project>();
        private static Queue<Project> unloaded = new Queue<Project>();
        //private XmlNode xproject;
        List<Source> sources = new List<Source>();
        Dictionary<string, string> macros = new Dictionary<string, string>();
        List<Reference> references = new List<Reference>();
        int roundTrip = 0;


        public string Type { get; private set; }
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
                Type = xType.Value;
            XmlNodeList xClones = xproject.SelectNodes("clone");
            bool allThere = true;
            List<Project> clone = new List<Project>();
            foreach (XmlNode xClone in xClones)
            {
                Project p = Add(xClone, SourcePath, this);
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
                    throw new Exception("Loop detected");

                }
                return;
            }
            foreach (Project p in clone)
            {
                sources.AddRange(p.sources);
                foreach (var pair in p.macros)
                    macros.Add(pair.Key, pair.Value);
                references.AddRange(p.references);

                if (Type == null)
                    Type = p.Type;

            }
            loaded = true;

            XmlNodeList xsources = xproject.SelectNodes("source");
            foreach (XmlNode xsource in xsources)
            {
                AddSource(xsource);
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