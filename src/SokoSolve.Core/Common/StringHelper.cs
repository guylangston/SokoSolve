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
        
        static readonly string[] SizeSuffixes = 
            { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(long value, int decimalPlaces = 1) => SizeSuffix((ulong)value, decimalPlaces);
        public static string SizeSuffix(ulong value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag          += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", 
                adjustedSize, 
                SizeSuffixes[mag]);
        }
    }
}