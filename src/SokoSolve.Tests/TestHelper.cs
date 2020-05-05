using System;
using System.IO;
using Xunit;

namespace SokoSolve.Tests
{
    public static class TestHelper
    {
        public static string GetDataPath()
        {
            if (Environment.CurrentDirectory.EndsWith("src\\SokoSolve.Tests\bin\\Debug\\netcoreapp3.0"))
                return @"../../../../../data/";
            
            if (Environment.CurrentDirectory.EndsWith("src/SokoSolve.Tests/bin/Debug/netcoreapp3.0"))
                return @"../../../../../data/";

            if (Directory.Exists(@"C:\Projects\SokoSolve\")) return @"C:\Projects\SokoSolve\data\";

            throw new Exception("Unable to find data path");
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