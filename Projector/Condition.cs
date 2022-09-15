using System.Xml;

namespace Projector
{

	/// <summary>
	/// Condition for certain operations declared by platform-name and config-name. If both are set, they are combined via and
	/// </summary>
	public readonly struct Condition
	{
		/// <summary>
		/// Platform target to be matched. Platform.None if not enabled (always true)
		/// </summary>
		public Platform IfPlatform { get; }
		/// <summary>
		/// Configuration name target to be matched. null if not enabled (always true)
		/// </summary>
		public string? IfConfig { get; }

		public Condition(Platform ifPlatform, string ifConfig)
		{
			IfPlatform = ifPlatform;
			IfConfig = ifConfig;
		}

		public Condition(XmlNode node, Solution domain, Project warnWhom)
		{
			XmlNode? ifPlatform = node.Attributes?.GetNamedItem("if_platform");
			try
			{
				if (ifPlatform is not null && (ifPlatform.Value?.Length ?? 0) > 0)
					IfPlatform = (Platform)Enum.Parse(typeof(Platform), ifPlatform.Value!);
				else
					IfPlatform = Platform.None;
			}
			catch
			{
				if (ifPlatform?.Value == "x32")
					IfPlatform = Platform.x86;
				else
				{
					warnWhom.Warn(domain, "Unable to decode condition platform '" + ifPlatform?.Value + "'. Supported values are ARM, x32, and x64");
					IfPlatform = Platform.None;
				}
			}
			XmlNode? ifConfig = node.Attributes?.GetNamedItem("if_config");
			if (ifConfig is not null && (ifConfig.Value?.Length ?? 0) > 0)
				IfConfig = ifConfig.Value;
			else
				IfConfig = null;
		}

		public override bool Equals(object? obj)
		{
			if (!(obj is Condition))
				return false;
			Condition other = (Condition)obj;
			return other == this;
		}

		public override int GetHashCode()
		{
			int hash = 17;
			//if (IfPlatform != Pla)
			hash = hash * 31 + IfPlatform.GetHashCode();
			if (IfConfig is not null)
				hash = hash * 31 + IfConfig.GetHashCode();
			return hash;
		}

		public static bool operator ==(Condition a, Condition b)
		{
			return a.IfPlatform == b.IfPlatform && a.IfConfig == b.IfConfig;
		}
		public static bool operator !=(Condition a, Condition b)
		{
			return a.IfPlatform != b.IfPlatform || a.IfConfig != b.IfConfig;
		}

		public override string ToString()
		{
			return "if (" + (IfPlatform != Platform.None ? IfPlatform.ToString() : "") + "," + (IfConfig ?? "") + ")";
		}

		public bool AlwaysTrue
		{
			get { return IfPlatform == Platform.None && IfConfig == null; }
		}

		public bool Excludes(Condition other)
		{
			bool differentA = IfPlatform != Platform.None && other.IfPlatform != Platform.None && IfPlatform != other.IfPlatform;
			bool differentB = IfConfig is not null && other.IfConfig is not null && IfConfig != other.IfConfig;
			return differentA || differentB;
		}

		public bool Test(Configuration config)
		{
			return (IfPlatform == Platform.None || IfPlatform == config.Platform)
					&&
					(IfConfig == null || IfConfig == config.Name);
		}

	}


}