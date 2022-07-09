using System;
using System.Collections.Generic;
using System.IO;

namespace Projector
{
	public class DependencyNode
	{
		public readonly FilePath File;
		public readonly bool DoCompile;
		public readonly Project.CodeGroup? Group;
		public List<Project> Parents { get; private set; } = new List<Project>();
		public Dictionary<FilePath, Tuple<DependencyNode, string>> Dependencies { get; private set; } = new ();

		public DependencyNode(Project? parent, FilePath file, Project.CodeGroup? group)
		{
			File = file;
			DoCompile = group == Project.c || group == Project.cpp;
			Group = group;
			if (parent is not null)
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