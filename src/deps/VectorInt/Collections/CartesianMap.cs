using System.Collections;
using System.Collections.Generic;

namespace VectorInt.Collections
{
    public class CartesianMap<T> : IReadOnlyCartesianMap<T>
    {
        public CartesianMap(int width, int height)
        {
            Width = width;
            Height = height;
            Size = new VectorInt2(width, height);
            inner = new T[width,height];
        }

        public CartesianMap(IReadOnlyCartesianMap<T> shallowCopy)
        {
            Width = shallowCopy.Width;
            Height = shallowCopy.Height;
            Size = shallowCopy.Size;
            inner = new T[shallowCopy.Width, shallowCopy.Height];
            foreach (var (p, v) in shallowCopy.ForEach())
            {
                inner[p.X, p.Y] = v;
            }
        }

        private readonly T[,] inner;
        
        public int Width { get; }

        public int Height { get; }

        public VectorInt2 Size { get; }

        public T this[int x, int y]
        {
            get => inner[x, y];
            set => inner[x, y] = value;
        }

        public T this[VectorInt2 p] 
        {
            get => inner[p.X, p.Y];
            set => inner[p.X, p.Y] = value;
        }

        public IEnumerable<T> ForEachValue()
        {
            for (var xx = 0; xx < Width; xx++)
                for (var yy = 0; yy < Height; yy++)
                    yield return inner[xx, yy];
        }

        public IEnumerable<(VectorInt2 Position, T Value)> ForEach()
        {
            for (var xx = 0; xx < Width; xx++)
                for (var yy = 0; yy < Height; yy++)
                   yield return (new VectorInt2(xx, yy), inner[xx, yy]);
        }

        
    }
}