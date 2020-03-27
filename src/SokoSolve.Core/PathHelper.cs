using System;
using System.IO;

namespace SokoSolve.Core
{
    public class PathHelper
    {
        public string GetDataPath()
        {
            if (Environment.CurrentDirectory.EndsWith("src\\SokoSolve.Tests\\bin\\Debug\\netcoreapp3.0"))
                return @"../../../../../data/";
            
            
            if (Directory.Exists(@"../../data")) return @"../../data/";
            
            if (Directory.Exists(@"C:\Projects\SokoSolve\")) return @"C:\Projects\SokoSolve\data\";
            
            
            throw new Exception($"Unable to find 'data' path. Curr={Environment.CurrentDirectory}");
        }

        public string GetLibraryPath()  
        {
            return Path.Combine(GetDataPath(), "Lib");
        }

        public string GetRelDataPath(string rel)
        {
            return Path.Combine(GetDataPath(), rel);
        }
    }
}