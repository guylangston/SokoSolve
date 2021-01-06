using System.Collections.Generic;
using System.IO;

namespace TextRenderZ
{
    /// <summary>
    /// Multi Line funcs
    /// </summary>
    public static partial class StringUtil
    {



        public static string StripLineFeeds(string txt) => txt.Replace("\n", "").Replace("\r", "");

        public static IEnumerable<string> ToLines(string txt)
        {
            using (var r = new StringReader(txt))
            {
                string? l = null;
                while ((l = r.ReadLine()) != null)
                {
                    yield return l;
                }
            }
        }
        
        
    }
}