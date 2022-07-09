using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Projector
{

    /// <summary>
    /// Access class for the persistently memorized configuration state
    /// </summary>
    public static class PersistentState
    {

        /// <summary>
        /// Descriptor structure for persistently memorized solutions
        /// </summary>
        public readonly struct SolutionDescriptor
        {
            public string? Name { get; }
			public string? Domain { get; }
			public FilePath File { get; }

			public SolutionDescriptor(FilePath file, string? domain)
			{
				Domain = domain;
				File = file;
				Name = file.CoreName;
			}

			public override bool Equals(object? obj)
			{
				if (!(obj is SolutionDescriptor))
					return false;
				SolutionDescriptor other = (SolutionDescriptor)obj;
				return other.File.FullName == File.FullName;
			}

			public override string ToString()
			{
				return Domain is not null && Domain.Length > 0 ? Domain + "/" + Name : Name??"";
			}

			public override int GetHashCode()
			{
				return File.GetHashCode();
			}
		}


        /// <summary>
        /// Retrieves the revently used solutions, ordered from most to least recently used
        /// </summary>
        public static IEnumerable<SolutionDescriptor> Recent => recent;


		/// <summary>
		/// Persistent access to the last used toolset (required during solution building)
		/// </summary>
        public static string? Toolset
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

		
		private static FilePath stateFile = new FilePath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),"projector","persistentState.xml"));

		/// <summary>
		/// Fetches the full path of the used state file
		/// </summary>
        public static FilePath StateFile => stateFile;

		/// <summary>
		/// Restores the persistent state from file
		/// </summary>
		/// <returns>true, if a state could be restored, false otherwise</returns>
        public static bool Restore()
        {
            if (StateFile.Directory is null)
                throw new InvalidOperationException(nameof(StateFile.Directory)+" is null");

            if (!StateFile.Directory.Exists)
				StateFile.Directory.Create();
            if (StateFile.Exists)
            {
                var xreader = new XmlTextReader(StateFile.FullName);
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(xreader);
                XmlNodeList? xrecent = xdoc.SelectNodes("state/recent/solution");
                recent.Clear();
                HashSet<string?> recentKnown = new();
                if (xrecent is not null)
                    foreach (XmlNode xr in xrecent)
                    {
					    FilePath f = new FilePath(xr.InnerText);
                        if (f.Exists && !recentKnown.Contains(f.FullName))
                        {
						    XmlNode? xdomain = xr.Attributes?.GetNamedItem("domain");
						    SolutionDescriptor desc;
						    desc = new SolutionDescriptor(f, xdomain is not null ? xdomain.Value : null);

                            recentKnown.Add(f.FullName);
                            recent.Add(desc);
                        }
                    }
                outPaths.Clear();
                XmlNodeList? xtargets = xdoc.SelectNodes("state/solution/target");
                if (xtargets is not null)
                    foreach (XmlNode xt in xtargets)
                    {
                        XmlNode? xsolution = xt.Attributes?.GetNamedItem("solutionFile");
                        if (xsolution?.Value is null)
                            continue;
					    FilePath sol = new (xsolution.Value);
					    FilePath ot = new (xt.InnerText);
                        if (!sol.Exists)
                            continue;
                        if (!ot.DirectoryExists)
                            continue;
                        outPaths.Add(sol.FullName!, ot);
                    }
                XmlNode? xtoolset = xdoc.SelectSingleNode("state/toolset");
                if (xtoolset is not null)
                    toolset = xtoolset.InnerText;
                xreader.Close();
				return true;
            }
			return false;
        }
        private static List<SolutionDescriptor> recent = new ();
        private static Dictionary<string, FilePath> outPaths = new ();
        private static string? toolset;

        /// <summary>
        /// Writes the local state to XML
        /// </summary>
        private static void Backup()
        {
            using (XmlWriter writer = XmlWriter.Create(StateFile.FullName))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("state");
                    writer.WriteStartElement("recent");
                        foreach (var r in recent)
						{
							writer.WriteStartElement("solution");
							writer.WriteAttributeString("domain", r.Domain);
							writer.WriteString(r.File.FullName);
							writer.WriteEndElement();
						}
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

        /// <summary>
        /// Moves the specified solution to the top of recently opened solutions.
        /// </summary>
        /// <param name="desc">Solution descriptor</param>
        /// <returns>True if the solution was newly added to the history, false if merely moved to the top</returns>
        public static bool MemorizeRecent(SolutionDescriptor desc)
        {
			bool newRecent = !recent.Remove(desc);
			recent.Insert(0, desc);
            Backup();
            return newRecent;
        }

        /// <summary>
        /// Purges the memory of all recently opened solutions
        /// </summary>
        public static void ClearRecent()
        {
            recent.Clear();
            Backup();
        }

        /// <summary>
        /// Queries the last memorized output path for the specified solution file
        /// </summary>
        /// <param name="solutionFile">Path to the solution file to determine the output path of</param>
        /// <returns>Last memorized output path. May be empty. Never null</returns>
        public static FilePath? GetOutPathFor(FilePath solutionFile)
        {
			FilePath? rs;
            if (solutionFile.FullName is not null && outPaths.TryGetValue(solutionFile.FullName, out rs))
                return rs;
            return null;
        }

        /// <summary>
        /// Updates the output path of a solution
        /// </summary>
        /// <param name="solutionSourceFile">Path to the solution file to memorize the output path of</param>
        /// <param name="solutionOutFile">Output path</param>
        public static void SetOutPathFor(FilePath solutionSourceFile, FilePath solutionOutFile)
        {
            if (outPaths.ContainsKey(solutionSourceFile.FullName))
            {
                if (outPaths[solutionSourceFile.FullName] == solutionOutFile)
                    return;
                outPaths[solutionSourceFile.FullName] = solutionOutFile;
            }
            else
                outPaths.Add(solutionSourceFile.FullName, solutionOutFile);
            Backup();
        }
    }
}