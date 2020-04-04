using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Common
{
    public static class ListHelper
    {
        public  static void InsertSorted<T>(this LinkedList<T> linked, T i, Comparison<T> comparer)
        {
            if (linked.Count == 0)
            {
                linked.AddFirst(i);
                return;
            }

            var current = linked.First;

            while (current != null &&  comparer(current.Value, i) <= 0) 
                current = current.Next;

            if (current == null)
            {
                linked.AddLast(i);
            }
            else
            {
                linked.AddBefore(current, i);
            }
        }
        
        public  static T FindInSorted<T>(this LinkedList<T> linked, T i, Comparison<T> comparer)
        {
            if (linked.Count == 0)
            {
                return default;
            }

            var current = linked.First;

            while(current != null)
            {
                var c = comparer(current.Value, i);
                if (c == 0) return current.Value;
                if (c > 0) return default;
                current = current.Next;
            }
            return default;
        }
        
        public static bool IsSorted<T>(IReadOnlyList<T> block, Comparison<T> compare)
        {
            if (block.Count == 0) return true;
            var x = block[0];
            for (int i = 1; i < block.Count-1; i++)
            {
                if (compare(x, block[i]) > 0) return false;
                x = block[i];
            }

            return true;
        }
        
        public static bool IsSorted<T>(IReadOnlyCollection<T> block, IComparer<T> compare)
        {
            if (block.Count < 2) return true;

            var prev = block.First();
            foreach (var curr in block.Skip(1))
            {
                if (compare.Compare(prev, curr) > 0) return false;
            }

            return true;
        }
    }
}