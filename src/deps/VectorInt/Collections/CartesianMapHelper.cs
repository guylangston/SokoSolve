using System.Linq;
using System.Runtime.CompilerServices;

namespace VectorInt.Collections
{
    public static class CartesianMapHelper 
    {
        
        public static bool Contains<T>(this IReadOnlyCartesianMap<T> map, VectorInt2 p)
        {
            if (map is SparseCartesianMap<T> sp)
            {
                return sp.Contains(p);
            }
            
            if (p.X < 0 || p.Y < 0) return false;
            if (p.X >= map.Width || p.Y >= map.Height) return false;
            return true;
        }
        
        public static int Count<T>(this IReadOnlyCartesianMap<T> map, T instance) => Enumerable.Count(map.ForEach(), x => object.Equals(instance, x.Value));

        // This is just what LINQ does?
//        public static  int Count<T>(IReadOnlyCartesianMap<T> map, T items)
//        {
//            var cc = 0;
//            foreach (var c in this)
//                if (c.Cell.Equals(state))
//                    cc++;
//            return cc;
//        }
    }
}