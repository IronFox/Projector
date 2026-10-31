using System;

namespace Projector
{
    public struct ToolsetVersion
    {
		public int Major { get; }
		public int Minor { get; }
		public string VSName { get; }
		/// <summary>
		/// Major version of the Visual Studio product that ships this toolset (12 = VS2013, ..., 17 = VS2022, 18 = VS2026)
		/// </summary>
		public int VSMajorVersion { get; }
		public string? WindowsTargetPlatformVersion { get; }
		public string? Path { get; }


		/// <summary>
		/// Value of the ToolsVersion attribute in generated .vcxproj files
		/// </summary>
		public string OutXMLText
		{
			get
			{
				return VSMajorVersion + ".0";
			}
		}

		/// <summary>
		/// Platform toolset name, e.g. v143 for VS2022 or v145 for VS2026
		/// </summary>
		public string PlatformToolset => "v" + (Major * 10 + Minor);

		/// <summary>
		/// Version comment line written into generated .sln files (selects the Visual Studio version launched by the VS version selector)
		/// </summary>
		public string SolutionVersionComment
		{
			get
			{
				switch (VSMajorVersion)
				{
					case 12:
						return "# Visual Studio 2013";
					case 14:
						return "# Visual Studio 14";
					default:
						return "# Visual Studio Version " + VSMajorVersion;
				}
			}
		}

		/// <summary>
		/// VisualStudioVersion value written into generated .sln files
		/// </summary>
		public string SolutionVisualStudioVersion => VSMajorVersion + ".0";


		public ToolsetVersion(int major, int minor, string vsName, int vsMajorVersion, string? windowsTargetPlatformVersion, string? path)
        {
            Major = major;
            Minor = minor;
			VSName = vsName;
			VSMajorVersion = vsMajorVersion;
			Path = path;
			WindowsTargetPlatformVersion = windowsTargetPlatformVersion;
		}



		public override string ToString()
        {
			return Major + "." + Minor+" ("+ VSName+")";
        }

		public static bool operator ==(ToolsetVersion a, ToolsetVersion b)
		{
			return a.Major == b.Major && a.Minor == b.Minor && a.VSName == b.VSName;
		}

		public static bool operator !=(ToolsetVersion a, ToolsetVersion b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return ((VSName.GetHashCode() * 17 + Major) * 17 + Minor);
		}

		public override bool Equals(object? obj)
		{
			if (obj is ToolsetVersion tv)
			{
				return this == tv;
			}
			if (obj is string s)
			{
				return ToString() == s;
			}
			return false;
		}
	}


	
}