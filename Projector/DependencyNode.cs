using System;
using System.Collections.Generic;
using System.IO;

namespace Projector
{
	public class DependencyNode
	{
		public readonly File File;
		public readonly bool DoCompile;
		public List<Project> Parents { get; private set; } = new List<Project>();
		public Dictionary<File, Tuple<DependencyNode, string>> Dependencies { get; private set; } = new Dictionary<File, Tuple<DependencyNode, string>>();

		public DependencyNode(Project parent, File file, bool compile)
		{
			this.File = file;
			this.DoCompile = compile;
			Parents.Add(parent);
		}

		internal IEnumerable<Tuple<DependencyNode, string>> GetRecursiveDependencies(int depth)
		{
			if (depth > 0)
				foreach (var d in Dependencies)
				{
					yield return d.Value;
					foreach (var d2 in d.Value.Item1.GetRecursiveDependencies(depth - 1))
						yield return d2;
				}
		}
	}
}