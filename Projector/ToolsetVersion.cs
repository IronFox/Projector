﻿using System;

namespace Projector
{
    public struct ToolsetVersion
    {
        public readonly int Major,
                        Minor;
		public readonly string VSName,Path, WindowsTargetPlatformVersion;
		

		public string OutXMLText
		{
			get
			{
				return Major + "." + Minor;
			}
		}
			

		public ToolsetVersion(int major, int minor, string vsName, string windowsTargetPlatformVersion, string path)
        {
            Major = major;
            Minor = minor;
			VSName = vsName;
			Path = path;
			WindowsTargetPlatformVersion = windowsTargetPlatformVersion;
		}

		public ToolsetVersion(int major, int minor, string vsName, string windowsTargetPlatformVersion) : this(major,minor,vsName, windowsTargetPlatformVersion, null)
		{}



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

		public override bool Equals(object obj)
		{
			if (obj is ToolsetVersion)
			{
				return this == (ToolsetVersion)obj;
			}
			if (obj is string)
			{
				return ToString() == (string)obj;
			}
			return false;
		}
	}


	
}