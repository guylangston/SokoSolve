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
    }
}
