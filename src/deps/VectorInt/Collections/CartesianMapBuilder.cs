using System.Collections.Generic;
using System.Linq;

namespace VectorInt.Collections
{
    public static class CartesianMapBuilder
    {
        public static CartesianMap<T> Create<T>(IReadOnlyList<IReadOnlyList<T>> lines)
        {
            var maxX = lines.Max(x => x.Count);
            var map = new CartesianMap<T>(maxX, lines.Count);
            for (int yIndex = 0; yIndex < lines.Count; yIndex++)
            {
                for (int xIndex = 0; xIndex < lines[yIndex].Count; xIndex++)
                {
                    map[xIndex, yIndex] = lines[yIndex][xIndex];
                }
            }

            return map;
        }
        
        public static CartesianMap<T> Create<T>(IReadOnlyCollection<(int x, int y, T v)> points)
        {
            var maxX = points.Max(x => x.x);
            var maxY = points.Max(x => x.y);
            
            var map = new CartesianMap<T>(maxX, maxY);
            foreach (var (x, y, v) in points)
            {
                map[x, y] = v;
            }

            return map;
        }
    }
}