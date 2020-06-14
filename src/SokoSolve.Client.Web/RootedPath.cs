using System;
using System.IO;

namespace SokoSolve.Client.Web
{
    public class RootedPath
    {
        private RootedPath(string root)
        {
            if (System.IO.Directory.Exists(root))
            {
                Root = new System.IO.DirectoryInfo(root).FullName;
                Sep = Root.Contains('/') ? '/' :
                    Root.Contains('\\') ? '\\' : throw new Exception("UnKnown Seperator");
                AltSep = Sep == '/' ? '\\' : '/';
                return;
            }    
            
            throw new Exception($"Could not find any root path '{Root}'. Curr: '{Environment.CurrentDirectory}'");
        }
        
        public static RootedPath FromAnyOf(params string[] roots)
        {
            foreach (var root in roots)
            {
                if (System.IO.Directory.Exists(root))
                {
                    return new RootedPath(root);
                }    
            }
            throw new Exception($"Could not find any root path '{string.Join(", ", roots)}'. Curr: '{Environment.CurrentDirectory}'");
        }

        public static RootedPath ScanBack(string name, string path = null /* curr dir */)
        {
            var p = path ?? Environment.CurrentDirectory;
            while (p != null && Directory.Exists(p))
            {
                var pp = System.IO.Path.GetFileName(p); // returns directory name (leaf node)
                if (string.Equals(pp, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new RootedPath(p);
                }

                p = System.IO.Directory.GetParent(p)?.FullName;
            }
            throw new Exception($"Could not find any root path '{name}'. Curr: '{Environment.CurrentDirectory}'");
        }

        public string Root { get;  }
        public char Sep { get; }
        public char AltSep { get; }

        public enum CheckType
        {
            None,
            FileExists,
            DirectoryExists,
            IsDirectory
        }

        public string GetRel(string p) => System.IO.Path.Combine(Root, p.Replace(AltSep, Sep));
    }
}