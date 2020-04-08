using System;
using System.Collections.Generic;
using System.IO;

namespace SokoSolve.Core.Common
{
    public class StringHelper
    {
        public static string Truncate(string txt, int max, string elipse = "...")
        {
            if (txt == null) return null;
            if (txt.Length < max) return txt;
            return txt.Substring(0, max - elipse.Length) + elipse;
        }

        public static string StripLineFeeds(string txt) => txt.Replace("\n", "").Replace("\r", "");

        public static IEnumerable<string> ToLines(string txt)
        {
            using (var r = new StringReader(txt))
            {
                string l = null;
                while ((l = r.ReadLine()) != null)
                {
                    yield return l;
                }
            }
        }
    }
}