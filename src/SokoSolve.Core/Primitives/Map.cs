using System;
using System.Collections.Generic;
using System.Text;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Core.Primitives
{

    
    public class Map<T> : ICartesianMap<T>
    {
        private readonly T[,] map;


        public Map(int sizeX, int sizeY)
        {
            map = new T[sizeX, sizeY];
        }

        public Map(VectorInt2 aSize) : this(aSize.X, aSize.Y)
        {
        }

        public Map(ICartesianMap<T> copy)
            : this(copy.Size.X, copy.Size.Y)
        {
            for (var cy = 0; cy < copy.Size.Y; cy++)
            for (var cx = 0; cx < copy.Size.X; cx++)
                this[cx, cy] = copy[cx, cy];
        }


        public T this[VectorInt2 pos]
        {
            get => this[pos.X, pos.Y];
            set => this[pos.X, pos.Y] = value;
        }

        public T this[int pX, int pY]
        {
            get => map[pX, pY];
            set => map[pX, pY] = value;
        }

        public IEnumerable<T> ForEachValue()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(VectorInt2 Position, T Value)> ForEach()
        {
            throw new NotImplementedException();
        }

        public int Width => map.GetLength(0);
        public int Height => map.GetLength(1);
        public VectorInt2 Size => new VectorInt2(Width, Height);

        
        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var cy = 0; cy < Size.Y; cy++)
            {
                for (var cx = 0; cx < Size.X; cx++) sb.Append(this[cx, cy]);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void Fill(T c)
        {
            for (var cy = 0; cy < Size.Y; cy++)
            for (var cx = 0; cx < Size.X; cx++)
                this[cx, cy] = c;
        }
    }
}