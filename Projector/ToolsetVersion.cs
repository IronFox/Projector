using System;

namespace Projector
{
    public struct ToolsetVersion
    {
        public readonly int Major,
                        Minor;

        public ToolsetVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public ToolsetVersion(string strVersion)
        {
            string source = strVersion;
            int at = strVersion.IndexOf(' ');
            if (at >= 0)
                strVersion = strVersion.Substring(0, at);
            at = strVersion.IndexOf('.');
            if (at >= 0)
            {
                string minor = strVersion.Substring(at + 1);
                if (!int.TryParse(minor, out Minor))
                {
                    throw new Exception("Internal Error: Unable to decode minor toolset-version from specified toolset '" + minor + "' (in '"+source+"')");
                }
                strVersion = strVersion.Substring(0, at);
            }
            else
                Minor = 0;
            if (!int.TryParse(strVersion, out Major))
            {
                throw new Exception("Internal Error: Unable to decode major toolset-version from specified toolset '" + strVersion + "' (in '" + source + "')");
            }
        }

        public override string ToString()
        {
            return Major + "." + Minor;
        }
    }
}