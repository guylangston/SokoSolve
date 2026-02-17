using System;
using System.IO;

namespace SokoSolve.Core
{
    public class PathHelper
    {
        public string GetDataPath()
        {
            var cp = Environment.CurrentDirectory.Replace('\\', '/');

            var idx = cp.IndexOf("/SokoSolve/", StringComparison.Ordinal);
            if (idx > 0)
            {
                return cp[0..(idx + "/SokoSolve/".Length)] + "data";
            }

            if (cp.EndsWith("/bin/Debug/netcoreapp3.1") ||
                cp.EndsWith("/bin/Debug/netcoreapp3.0") ||
                cp.EndsWith("/bin/Release/netcoreapp3.1") ||
                cp.EndsWith("/bin/Release/netcoreapp3.0"))
                return @"../../../../../data/";

            if (Directory.Exists(@"../../data")) return @"../../data/";

            if (Directory.Exists(@"C:\Projects\SokoSolve\")) return @"C:\Projects\SokoSolve\data\";

            throw new Exception($"Unable to find 'data' path. Curr={Environment.CurrentDirectory}");
        }

        public string GetLibraryPath() => Path.Combine(GetDataPath(), "Lib");
        public string GetRelDataPath(string rel) => Path.Combine(GetDataPath(), rel);
    }
}
