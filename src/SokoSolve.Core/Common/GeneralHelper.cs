using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SokoSolve.Core.Common
{
    public static class GeneralHelper
    {
        public static IEnumerable<Tuple<T, T>> OffsetWalk<T>(IEnumerable<T> items) where T : class
        {
            var prev = items.FirstOrDefault();
            if (prev != null)
                foreach (var curr in items.Skip(1))
                {
                    yield return new Tuple<T, T>(prev, curr);

                    prev = curr;
                }
        }

        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            if (list is List<T> l) return l.IndexOf(item);
            if (list is T[] a) return Array.IndexOf(a, item);
            
            for (int i = 0; i < list.Count; i++)
            {
                if (object.Equals(list[i], item)) return i;
            }

            return -1;
        }

        public static IEnumerable<(T item, int index)> WithIndex<T>(this IEnumerable<T> items)
        {
            var cc = 0;
            foreach (var item in items)
            {
                yield return (item, cc++);
            }
        }

        public static IEnumerable<(T a, T b)> PairUp<T>(this IReadOnlyList<T> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (i < items.Count-1)
                {
                    yield return (items[i], items[i + 1]);
                }
                else
                {
                    yield return (items[i], default);
                }
            }
        }

        public static string? ToStringConcat(IEnumerable? list)
        {
            if (list == null) return null;
            var sb = new StringBuilder();
            var cc = 0;
            foreach (var obj in list)
            {
                if (cc++ > 0) sb.Append(",");
                if (obj != null) sb.Append(obj);
            }

            return sb.ToString();
        }

        public static string Humanize(this TimeSpan span)
        {
            if (span.TotalSeconds < 1) return $"{span.Milliseconds} ms";
            if (span.TotalSeconds < 2) return $"{span.Seconds:0.0} sec";
            if (span.TotalMinutes < 1) return $"{span.Seconds} sec";
            if (span.TotalHours < 1)
            {
                if (span.Seconds == 0) return $"{span.Minutes} min";
                return $"{span.Minutes} min, {span.Seconds} sec";
            }
            if (span.TotalDays < 1) return $"{span.Hours} hr, {span.Minutes} min";
            if (span.TotalDays > 365) return $"{(int) span.TotalDays / 365} yrs, {(int) span.TotalDays % 365} days";

            if (span.Hours == 0) return $"{span.Days} days";
            return $"{span.Days} days, {span.Hours} hr";
        }
    }
}