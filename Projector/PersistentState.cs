using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Projector
{
    internal class PersistentState
    {




        public static IEnumerable<FileInfo> Recent { get { return recent; } }

        public static string Toolset
        {
            get
            {
                return toolset;
            }
            set
            {
                if (toolset == value)
                    return;
                toolset = value;
                Backup();

            }
        }


        public static FileInfo StateFile { get; private set; } = new FileInfo("persistentState.xml");

        public static void Restore()
        {
            if (StateFile.Exists)
            {
                var xreader = new XmlTextReader(StateFile.FullName);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xreader);
                XmlNodeList xrecent = xdoc.SelectNodes("state/recent/solution");
                recent.Clear();
                HashSet<string> recentKnown = new HashSet<string>();
                foreach (XmlNode xr in xrecent)
                {
                    FileInfo f = new FileInfo(xr.InnerText);
                    if (!recentKnown.Contains(f.FullName))
                    {
                        recentKnown.Add(f.FullName);
                        recent.Add(f);
                    }
                }
                outPaths.Clear();
                XmlNodeList xtargets = xdoc.SelectNodes("state/solution/target");
                foreach (XmlNode xt in xtargets)
                {
                    XmlNode xsolution = xt.Attributes.GetNamedItem("solutionFile");
                    if (xsolution == null)
                        continue;
                    FileInfo sol = new FileInfo(xsolution.Value);
                    FileInfo ot = new FileInfo(xt.InnerText);
                    if (!sol.Exists)
                        continue;
                    if (!ot.Directory.Exists)
                        continue;
                    outPaths.Add(sol.FullName, ot);
                }
                XmlNode xtoolset = xdoc.SelectSingleNode("state/toolset");
                if (xtoolset != null)
                    toolset = xtoolset.InnerText;
                xreader.Close();
            }
        }
        private static List<FileInfo> recent = new List<FileInfo>();
        private static Dictionary<string, FileInfo> outPaths = new Dictionary<string, FileInfo>();
        private static string toolset;

        public static void Backup()
        {
            using (XmlWriter writer = XmlWriter.Create(StateFile.FullName))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("state");
                    writer.WriteStartElement("recent");
                        foreach (var r in recent)
                            writer.WriteElementString("solution", r.FullName);
                    writer.WriteEndElement();
                    writer.WriteStartElement("solution");
                        foreach (var p in outPaths)
                        {
                            writer.WriteStartElement("target");
                            writer.WriteAttributeString("solutionFile", p.Key);
                            writer.WriteString(p.Value.FullName);
                            writer.WriteEndElement();
                        }
                    writer.WriteEndElement();
                    writer.WriteElementString("toolset", toolset);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Close();
            }
        }

        public static void MemorizeRecent(FileInfo file)
        {
            recent.Remove(file);
            recent.Insert(0, file);
            Backup();
        }

        internal static void ClearRecent()
        {
            recent.Clear();
            Backup();
        }

        public static FileInfo GetOutPathFor(FileInfo solutionFile)
        {
            FileInfo rs;
            if (outPaths.TryGetValue(solutionFile.FullName, out rs))
                return rs;
            return null;
        }

        public static void SetOutPathFor(FileInfo solutionFile, FileInfo fileInfo)
        {
            outPaths.Add(solutionFile.FullName, fileInfo);
            Backup();
        }
    }
}