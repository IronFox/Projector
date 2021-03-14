using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Projector
{
	namespace XmlOut
	{
		/// <summary>
		/// Auto-indenting XML section. May contain children or arbitrary text.
		/// Must be disposed of after use
		/// </summary>
		public class Section : IDisposable
		{
			/// <summary>
			/// Node name
			/// </summary>
			public string Name { get; private set; }
			private readonly Section parent;
			private readonly StreamWriter writer;
			private bool headerWritten = false;

			private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

			/// <summary>
			/// Creates a document-level root section without name
			/// </summary>
			/// <param name="writer">Writer to send text to</param>
			public Section(StreamWriter writer)
			{
				this.writer = writer;
			}

			private Section(Section parent, string name)
			{
				Name = name;
				this.parent = parent;
			}

			private bool NeedHeader(bool close)
			{
				if (parent != null && !headerWritten)
				{
					headerWritten = true;
					if (parameters.Count == 0)
					{
						if (!close)
							parent.WriteLine($"<{Name}>");
						else
							parent.WriteLine($"<{Name} />");
					}
					else
					{
						StringBuilder builder = new StringBuilder();
						builder.Append($"<{Name}");
						foreach (var pair in parameters)
						{
							builder.Append(" ").Append(pair.Key).Append("=").Append(Quote(pair.Value));
						}
						if (close)
							builder.Append(" /");
						builder.Append(">");
						parent.WriteLine(builder.ToString());
					}
					return true;
				}
				return false;
			}

			/// <summary>
			/// Quotes text for parameter output
			/// </summary>
			/// <param name="value"></param>
			/// <returns></returns>
			internal static string Quote(string value)
			{
				return $"\"{value}\"";  //for now prolly good enough
			}

			/// <summary>
			/// Appends a new line to the body of the local section
			/// </summary>
			/// <param name="text">Line text</param>
			public void WriteLine(string text)
			{
				NeedHeader(false);

				WriteLine(0, text);
			}

			private void WriteLine(int indentation, string text)
			{
				if (parent != null)
					parent.WriteLine(indentation + 1, text);
				else
				{
					for (int i = 0; i < indentation; i++)
						writer.Write('\t');
					writer.WriteLine(text);
				}
			}

			public void Dispose()
			{
				if (NeedHeader(true))
					return;
				if (parent != null)
					parent.WriteLine($"</{Name}>");
			}

			/// <summary>
			/// Appends a new sub-section to the local XML node
			/// </summary>
			/// <param name="name">XML sub-section name</param>
			/// <returns>New section</returns>
			public Section SubSection(string name)
			{
				return new Section(this, name);
			}

			/// <summary>
			/// Creates a new single-line XML node.
			/// Must be disposed after use
			/// </summary>
			/// <param name="name">XML sub-node name</param>
			/// <returns>New single-line XML node</returns>
			public Line SingleLine(string name)
			{
				return new Line(this, name);
			}

			/// <summary>
			/// Creates a new single-line XML node
			/// and immediately disposes of it.
			/// </summary>
			/// <param name="name">Name of the single line XML node</param>
			/// <param name="content">Inner content</param>
			public void SingleLine(string name, bool content)
			{
				SingleLine(name, content ? "true" : "false");
			}

			/// <summary>
			/// Creates a new single-line XML node
			/// and immediately disposes of it.
			/// </summary>
			/// <param name="name">Name of the single line XML node</param>
			/// <param name="content">Inner content</param>
			public void SingleLine(string name, string content)
			{
				using (var line = SingleLine(name))
				{
					line.Write(content);
				}
			}

			/// <summary>
			/// Sets or updates an XML parameter of this node.
			/// Must not be called after any children or inner content
			/// was added.
			/// </summary>
			/// <param name="name">Parameter name (unique)</param>
			/// <param name="value">Value</param>
			public void AddParameter(string name, string value)
			{
				if (headerWritten)
					throw new InvalidOperationException("Trying add parameters to node but existing parameters were already written in " + this);
				parameters.Add(name, value);
			}

			public override string ToString()
			{
				return parent != null ? parent + "->" + Name : "*";
			}
		}
	}
}