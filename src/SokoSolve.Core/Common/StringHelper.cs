using System;

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
    }
}