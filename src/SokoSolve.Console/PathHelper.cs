using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokoSolve.Console
{
    public class PathHelper
    {
        public string GetDataPath()
        {
            if (Environment.CurrentDirectory.EndsWith("src\\SokoSolve.Tests\bin\\Debug\\netcoreapp3.0"))
            {
                return @"../../../../../data/";
            }
            
            if (Environment.CurrentDirectory.EndsWith("SokoSolve.Console"))
            {
                return @"../../data/";
            }

            if (System.IO.Directory.Exists(@"C:\Projects\SokoSolve\"))
            {
                return @"C:\Projects\SokoSolve\data\";
            }

            throw new Exception($"Unable to find data path. Current={Environment.CurrentDirectory}");
        }

        public string GetLibraryPath() => System.IO.Path.Combine(GetDataPath(), "Lib");

        public string GetRelDataPath(string rel)
        {
            return System.IO.Path.Combine(GetDataPath(), rel);
        }
    }
}
