using System;
using System.Collections.Generic;
using System.IO;

namespace Projector
{
    internal class PersistentState
    {

        private static List<FileInfo> recent = new List<FileInfo>();
        private static Dictionary<string, FileInfo> outPaths = new Dictionary<string, FileInfo>();

        public static IEnumerable<FileInfo> Recent { get { return recent; }  }

        public static void MemorizeRecent(FileInfo file)
        {
            recent.Remove(file);
            recent.Insert(0, file);
        }

        internal static void ClearRecent()
        {
            recent.Clear();
        }

        public static FileInfo GetOutPathFor(string solutionName)
        {
            FileInfo rs;
            if (outPaths.TryGetValue(solutionName, out rs))
                return rs;
            return null;
        }

        public static void SetOutPathFor(string solutionName, FileInfo fileInfo)
        {
            outPaths.Add(solutionName, fileInfo);
        }
    }
}