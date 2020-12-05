using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using Xunit;

namespace SokoSolve.Tests
{
    public static class TestHelper
    {
        public static string GetTestProjectPath()
        {
            var          curr        = Environment.CurrentDirectory;
            const string projectName = "SokoSolve.Tests";
            var          i           =curr.IndexOf(projectName, StringComparison.Ordinal);
            if (i < 0) throw new Exception($"Current Directory must include the project '{projectName}' : {curr}");

            return curr[0..(i + projectName.Length)] + Path.DirectorySeparatorChar;
        }
        
        public static string GetDataPath()  // rel project: ../../data 
        {
            var path = Path.Combine(GetTestProjectPath(), "../../data/");
            if (!Directory.Exists(path)) throw new Exception($"Cannot resolver data path (rel test project): {path}");
            return path;
        }

        public static string GetLibraryPath()
        {
            return Path.Combine(GetDataPath(), "Lib");
        }

        public static string GetRelDataPath(string rel)
        {
            return Path.Combine(GetDataPath(), rel);
        }
    }
}