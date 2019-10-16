using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VectorInt;

namespace SokoSolve.Core.Primitives
{
    public static class GridInt
    {
        public static IEnumerable<VectorInt2> WithSize(VectorInt2 size)
        {
            for (var x = 0; x < size.X; x++)
            for (var y = 0; y < size.Y; y++)
                yield return new VectorInt2(x, y);
        }
    }


    public interface IMap<T> : IEnumerable<Tuple<VectorInt2, T>>
    {
        T this[VectorInt2 pos] { get; set; }
        T this[int pX, int pY] { get; set; }
        VectorInt2 Size { get; }
    }

    public class Map<T> : IMap<T>
    {
        private readonly T[,] map;


        public Map(int sizeX, int sizeY)
        {
            map = new T[sizeX, sizeY];
        }

        public Map(VectorInt2 aSize) : this(aSize.X, aSize.Y)
        {
        }

        public Map(IMap<T> copy)
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

        public VectorInt2 Size => new VectorInt2(map.GetLength(0), map.GetLength(1));

        public IEnumerator<Tuple<VectorInt2, T>> GetEnumerator()
        {
            for (var cy = 0; cy < Size.Y; cy++)
            for (var cx = 0; cx < Size.X; cx++)
                yield return new Tuple<VectorInt2, T>(new VectorInt2(cx, cy), this[cx, cy]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

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