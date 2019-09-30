using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SokoSolve.Core.Common
{
    public static class GeneralHelper
    {
        public static IEnumerable<Tuple<T, T>> OffsetWalk<T>(IEnumerable<T> items)  where T: class 
        {
            var prev = items.FirstOrDefault();
            if (prev != null)
            {
                foreach (var curr in items.Skip(1))
                {
                    yield return new Tuple<T, T>(prev, curr);

                    prev = curr;
                }
            }
        }

        public static string ToStringConcat(IEnumerable list)
        {
            if (list == null) return null;
            var sb = new StringBuilder();
            int cc = 0;
            foreach (var obj in list)
            {
                if (cc++ > 0)
                {
                    sb.Append(",");
                }
                if (obj != null) sb.Append(obj.ToString());
            }
            return sb.ToString();
        }

        public static string Humanize(this TimeSpan span)
        {
            if (span.TotalSeconds < 1) return $"{span.Milliseconds} ms";
            if (span.TotalSeconds < 2) return $"{span.Seconds:0.0} sec";
            if (span.TotalMinutes < 1) return $"{span.Seconds} sec";
            if (span.TotalHours < 1) return $"{span.Minutes} min, {span.Seconds} sec";
            if (span.TotalDays < 1) return $"{span.Hours} hr, {span.Minutes} min";
            if (span.TotalDays > 365) return $"{(int)span.TotalDays/365} yrs, {(int)span.TotalDays % 365} days"; 

            if (span.Hours == 0) return $"{span.Days} days";
            return $"{span.Days} days, {span.Hours} hr";
        }
    }
}
