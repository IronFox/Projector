using System;
using System.Collections.Generic;
using System.Text;

namespace XmlWriter
{
	/// <summary>
	/// Single-line XML node. May have inner text-content and parameters but no children.
	/// Must be disposed of after use
	/// </summary>
	public class Line : IDisposable
	{
		private readonly Section parent;
		private readonly string name;
		private readonly StringBuilder builder = new StringBuilder();
		private bool anythingWritten = false;
		private bool headerWritten = false;


		private readonly Dictionary<string, string> parameters = new Dictionary<string, string>();

		private bool NeedHeader(bool close)
		{
			if (parent != null && !headerWritten)
			{
				headerWritten = true;
				if (parameters.Count == 0)
				{
					if (!close)
						builder.Append($"<{name}>");
					else
						builder.Append($"<{name} />");
				}
				else
				{
					builder.Append($"<{name}");
					foreach (var pair in parameters)
					{
						builder.Append(" ").Append(pair.Key).Append("=").Append(
							Section.Quote(pair.Value)
							);
					}
					if (close)
						builder.Append(" /");
					builder.Append(">");
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Constructs a new nested XML node
		/// </summary>
		/// <param name="parent">Containing section</param>
		/// <param name="name">Line tag name</param>
		internal Line(Section parent, string name)
		{
			this.parent = parent;
			this.name = name;

			//builder.Append($"<{name}>");
		}

		/// <summary>
		/// Appends text to inner text section of this node.
		/// Disallows subsequent parameter-setting
		/// </summary>
		/// <param name="text">Text to print</param>
		public void Write(string text)
		{
			if (text.Length == 0)
				return;
			NeedHeader(false);
			builder.Append(text);
			if (text.Length > 0)
				anythingWritten = true;
		}

		public void Dispose()
		{
			if (!NeedHeader(true))
				builder.Append($"</{name}>");
			parent.WriteLine(builder.ToString());
		}

		/// <summary>
		/// Appends text to the local XML node's inner text.
		/// If any text exists, the specified separator is printed first
		/// </summary>
		/// <param name="text"></param>
		/// <param name="separator"></param>
		public void SeparatedWrite(string text, string separator=";")
		{
			if (text.Length == 0)
				return;
			NeedHeader(false);
			if (anythingWritten)
				builder.Append(separator);
			builder.Append(text);
			anythingWritten = true;
		}

		/// <summary>
		/// Sets or updates a parameter of the local XML node
		/// </summary>
		/// <param name="name">Parameter name (unique)</param>
		/// <param name="value">Associated value</param>
		public void AddParameter(string name, string value)
		{
			parameters.Add(name, value);
		}
	}
}