using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextRenderZ
{
    /// <summary>
    /// Primtives
    /// </summary>
    public static partial class StringUtil
    {

        public static string Repeat(string s, int count)
        {
            var sb = new StringBuilder();
            for (int cc = 0; cc < count; cc++)
            {
                sb.Append(s);
            }
            return sb.ToString();

        }

        public static string? Truncate(string? txt, int max, string elipse = "...")
        {
            if (txt == null) return null;
            if (txt.Length < max) return txt;
            return txt.Substring(0, max - elipse.Length) + elipse;
        }

        public static string PadCentre(int size, string text)
        {
            if (text.Length >= size) return text;
            var half = (size - text.Length) / 2;
            var l    = text.PadLeft(half);
            return l.PadRight(size - l.Length);
        }

        public static string TakeMax(string text, int size)
        {
            if (text.Length < size) return text;
            return text.Substring(0, size);
        }

        public static string? TextBetween(string text, string start, string end)
        {
            var s = text.IndexOf(start, StringComparison.InvariantCultureIgnoreCase);
            if (s < 0) return null;
            var e = text.IndexOf(end, s, StringComparison.InvariantCultureIgnoreCase);
            if (e < 0) return null;
            return text.Substring(s + start.Length, e - (s + start.Length));
        }

        public static string TextBetween(string text, int start, int startLen, int end)
            => text.Substring(start + startLen, end - (start + startLen));

        public static string? Elipse(string? text, int max, string elipse = "...")
        {
            if (text == null) return null;
            text = text.Replace('\n', '|').Replace('\r', '|').Replace('\f', '|');
            if (text.Length <= max) return text;
            return text.Substring(0, max-elipse.Length) + elipse;
        }

        public static (string left, string right) SplitAtNotInclusive(string raw, int idx)
            => (raw.Substring(0, idx), raw.Substring(idx + 1, raw.Length - idx -1));

        public static (string left, string right) SplitAtInclusive(string raw, int idx)
            => (raw.Substring(0, idx), raw.Substring(idx , raw.Length - idx));

        public static string? TrimWhile(string? txt, Func<char, bool> removeIf)
        {
            if (txt == null) return null;

            var sb = new StringBuilder(txt);

            // Front
            while (sb.Length > 0 && removeIf(sb[0]))
            {
                sb.Remove(0, 1);
            }

            // End
            while (sb.Length > 0 && removeIf(sb[sb.Length-1]))
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }
    }

}
