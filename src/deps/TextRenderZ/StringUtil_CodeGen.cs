using System;
using System.Text;

namespace TextRenderZ
{
    /// <summary>
    /// Coding / CodeGen
    /// </summary>
    public static partial class StringUtil
    {
        public static string UnCamel(string text)
        {
            var sb = new StringBuilder(text);
            int cc = 0;
            while (cc < sb.Length - 1)
            {
                var next = sb[cc + 1];
                var curr = sb[cc];
                if (Char.IsLetter(curr) && Char.IsLower(curr) &&
                    Char.IsLetter(next) && Char.IsUpper(next))
                {
                    sb.Insert(cc + 1, ' ');
                    cc++;
                }
                cc++;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Format 'Text {variable} more text {another var}'
        /// </summary>
        public static string ParseAndReplaceVariables(string s, Func<string, string> renderVar)
        {
            var sb = new StringBuilder(s);

            var cc = sb.Length - 1;
            while (cc > 0)
            {
                if (sb[cc] == '}')
                {
                    var match = FindNext(cc);
                    if (match == -1) return sb.ToString();

                    var len = cc - match + 1;
                    var var = new char[len-2];
                    sb.CopyTo(match + 1, var, 0, len-2);

                    sb.Remove(match, len);
                    sb.Insert(match, renderVar(new string(var)));
                    cc = match -1;
                }
                else
                {
                    cc--;
                }

            }

            return sb.ToString();

            int FindNext(int i)
            {
                while (i >= 0)
                {
                    if (sb[i] == '{') return i;
                    i--;
                }

                return -1;
            }
        }

    }
}
