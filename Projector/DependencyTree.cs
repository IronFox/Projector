using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Projector
{
	public static class DependencyTree
	{
		private static Dictionary<File, DependencyNode> nodes = new Dictionary<File, DependencyNode>();


		private static HashSet<string> standardHeaders = new HashSet<string>(new string[]{
			"ppl.h",
			"iostream",
			"stdlib.h",
			"stdio.h",
			"vlc.h",
			"windef.h",
			"algorith.h",
			"winsock2.h",
			"windows.h",
			"string.h",
			"stdio.h",
			"pshpack.h",
			"pshpack1.h",
			"poppack.h",
			"cstdlib",
			"wchar.h",
			"cstdarg",
			"exception",
			"algorithm",
			"new",
			"initializer_list",
			"memory",
			"cmath",
			"functional",
			"time.h",
			"math.h",
			"atomic",
			"winbase.h",
			"Psapi.h",
			"unistd.h",
			"pthread.h",
			"malloc.h",
			"fcntl.h",
			"crtdbg.h",
			"stdarg.h",
			"stdint.h",
			"inttypes.h",
			"float.h",
			"crtdbg.h",
			"ctype.h",
			"wctype.h",
			"limits.h",
			"limits",
			"sstream",
			"mutex",
			"thread",
			"complex",
			"random",

		});
		public static void Clear()
		{
			nodes.Clear();
		}

		public static void RegisterNode(Project owner, File path, bool compile)
		{
			if (nodes.TryGetValue(path, out var node))
				node.Parents.Add(owner);
			else
				nodes.Add(path, new DependencyNode(owner, path, compile));
		}

		internal static void ParseDependencies()
		{
			foreach (DependencyNode node in nodes.Values)
			{
				Project parent = node.Parents[0];
				EventLog.Inform(null, parent, "Parsing " + node.File);
				node.Dependencies.Clear();
				string line;
				File file = node.File;
				System.IO.StreamReader read = new System.IO.StreamReader(file.FullName);
				int lineIndex = 0;
				while ((line = read.ReadLine()) != null)
				{
					lineIndex++;
					line = line.Trim();
					string original = line;
					if (!line.StartsWith("#include"))
						continue;
					line = line.Substring(8).Trim();
					if (line.StartsWith("\""))
					{
						line = line.Substring(1);
						int endAt = line.IndexOf('"');
						if (endAt == -1)
						{
							EventLog.Warn(null, parent, file.Name + ": Malformatted include directive in line " + lineIndex + " '" + original + "'");
							continue;
						}
						line = line.Substring(0, endAt);
						File candidate = new File(System.IO.Path.Combine(file.DirectoryName, line));
						if (!candidate.Exists)
							EventLog.Warn(null, parent, file.Name + ": Referenced file not found: '" + line + "' in line " + lineIndex);
						else
						{
							if (node.Dependencies.ContainsKey(candidate))
								continue;
							if (nodes.TryGetValue(candidate, out var other))
								node.Dependencies.Add(candidate,new Tuple<DependencyNode, string>(other, line));
							else
								node.Dependencies.Add(candidate,new Tuple<DependencyNode, string>(new DependencyNode(null, candidate, false), line));
						}
						continue;
					}

					if (line.StartsWith("<"))
					{
						line = line.Substring(1);
						int endAt = line.IndexOf('>');
						if (endAt == -1)
						{
							EventLog.Warn(null, parent, file.Name + ": Malformatted include directive in line " + lineIndex + " '" + original + "'");
							continue;
						}
						line = line.Substring(0, endAt);

						if (standardHeaders.Contains(line.ToLower()))
							continue;
						bool found = false;

						foreach (Project p in node.Parents)
						{
							foreach (string path in p.IncludedPaths)
							{
								File candidate = new File(Path.Combine(path, line));
								if (candidate.Exists)
								{
									found = true;
									if (!node.Dependencies.ContainsKey(candidate))
									{
										if (nodes.TryGetValue(candidate, out var other))
											node.Dependencies.Add(candidate,new Tuple<DependencyNode, string>(other, line));
										else
											node.Dependencies.Add(candidate, new Tuple<DependencyNode, string>(new DependencyNode(null, candidate, false), line));
									}
									break;
								}
							}
							if (found)
								break;
						}
						if (!found)
							EventLog.Warn(null, parent, file.Name + ": Dependency not found: '" + line + "' in line " + lineIndex);
					}

				}
			}

			foreach (DependencyNode node in nodes.Values)
			{
				if (node.DoCompile)
				{
					List<Tuple<DependencyNode, string>> newEntries = new List<Tuple<DependencyNode, string>>();
					foreach (Tuple<DependencyNode, string> d in node.GetRecursiveDependencies(10))
						if (!node.Dependencies.ContainsKey(d.Item1.File))
							newEntries.Add(d);
					foreach (var e in newEntries)
						if (!node.Dependencies.ContainsKey(e.Item1.File))
							node.Dependencies.Add(e.Item1.File, e);
				}
			}
		}

		private static string MakeRelativePath(DirectoryInfo from, DirectoryInfo to)
		{
			string[] s0 = from.FullName.Split(new char[] {'/','\\'});
			string[] s1 = to.FullName.Split(new char[] {'/','\\'});
			int cnt = 0;
			while (cnt < s0.Length && cnt < s1.Length && s0[cnt] == s1[cnt])
				cnt++;
			if (cnt > 0)
			{
				string relative = "";
				for (int i = cnt; i < s0.Length; i++)
					relative += ".." + Path.DirectorySeparatorChar;
				relative += s1.Skip(cnt).Fuse("" +Path.DirectorySeparatorChar);
				//if (relative.Length < to.FullName.Length)
					return relative;
			}
			return to.FullName;
		}

		public static void GenerateMakefiles()
		{
			Dictionary<Project, List<DependencyNode>> projects = new Dictionary<Project, List<DependencyNode>>();
			foreach (DependencyNode node in nodes.Values)
			{
				if (node.DoCompile)
				{
					foreach (Project p in node.Parents)
					{
						List<DependencyNode> list;
						if (!projects.TryGetValue(p, out list))
						{
							list = new List<DependencyNode>();
							projects.Add(p, list);
						}
						list.Add(node);
					}
				}
			}
			foreach (var scope in projects)
			{
				string outFile = Path.Combine(scope.Key.SourcePath.DirectoryName, scope.Key.Name + ".make");

				using (StreamWriter writer = new StreamWriter(new MemoryStream()))
				{
					foreach (var n in scope.Value)
					{
						string path = MakeRelativePath(scope.Key.SourcePath.Directory, n.File.Directory);

						writer.Write(Path.Combine(path, n.File.CoreName + ".o") + " : " + Path.Combine(path, n.File.Name));
						foreach (var d in n.Dependencies)
						{
							string path2 = MakeRelativePath(scope.Key.SourcePath.Directory, d.Value.Item1.File.Directory);
							writer.Write(" " + Path.Combine(path2, d.Value.Item1.File.Name));
						}
						writer.WriteLine();
					}
					Program.ExportToDisk(new File(outFile), writer);
				}
			}
		}
	}
}
 