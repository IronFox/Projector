using System;
using System.Collections.Generic;
using System.IO;

namespace Projector
{
    internal class PersistentState
    {

        private static List<FileInfo> recent = new List<FileInfo>();

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
    }
}