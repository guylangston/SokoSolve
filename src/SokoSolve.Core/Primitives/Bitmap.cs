using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Core.Primitives
{
    public interface IBitmap : IReadOnlyCartesianMap<bool>
    {
        new bool this[VectorInt2 pos] { get; set; }
        new bool this[int pX, int pY] { get; set; }
    }

    public class Bitmap : IBitmap, IEquatable<IBitmap>, IComparable<IBitmap>
    {
        private readonly uint[] map;
        private readonly VectorInt2 size;

        public Bitmap(int aSizeX, int aSizeY)
        {
            if (aSizeX > 32) throw new NotSupportedException("Only 32bit sizes are excepted");
            size = new VectorInt2(aSizeX, aSizeY);
            map = new uint[aSizeY];
        }

        public Bitmap(VectorInt2 aSize) : this(aSize.X, aSize.Y)
        {
        }

        /// <summary>
        ///     Copy Constructor. Deep copy.
        /// </summary>
        public Bitmap(IBitmap copy)
            : this(copy.Size.X, copy.Size.Y)
        {
            for (var cy = 0; cy < copy.Size.Y; cy++)
            for (var cx = 0; cx < copy.Size.X; cx++)
                this[cx, cy] = copy[cx, cy];
        }

        /// <summary>
        ///     Copy Constructor. Deep copy. Optimised
        /// </summary>
        public Bitmap(Bitmap copy)
        {
            size = copy.size;
            map = new uint[copy.map.Length];
            copy.map.CopyTo(map, 0);
        }

        public int        Width  => size.X;
        public int        Height => size.Y;
        public VectorInt2 Size   => size;
        public bool       IsZero => Count == 0; // Are any bits set? This is a fast function.

        /// <summary>
        ///     The number of 1's (set bits)
        /// </summary>
        public int Count
        {
            get
            {
                var result = 0;
                for (var ccy = 0; ccy < map.Length; ccy++)
                {
                    if (map[ccy] == 0) continue;
                    for (var ccx = 0; ccx < size.X; ccx++)
                        if (IsTrue(ccx, ccy))
                            result++;
                }

                return result;
            }
        }

       
        public bool this[int pX, int pY]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (map[pY] & (1 << pX)) > 0;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => map[pY] = value 
                    ? map[pY] | (uint) (1 << pX) 
                    : map[pY] & ~(uint) (1 << pX);
        }

        public bool this[VectorInt2 aPoint]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this[aPoint.X, aPoint.Y];
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => this[aPoint.X, aPoint.Y] = value;
        }


        public uint this[int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => map[y];
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTrue(int pX, int pY) => (map[pY] & (1 << pX)) > 0;

        public int CompareTo(IBitmap other)
        {
            for (var ccy = 0; ccy < map.Length; ccy++)
            for (var ccx = 0; ccx < size.X; ccx++)
            {
                var me = this[ccx, ccy];
                var you = other[ccx, ccy];
                if (me && !you) return 1;
                if (!me && you) return -1;
            }

            return 0;
        }

        public bool Equals(IBitmap rhs)
        {
            if (rhs == null) return false;
            if (size != rhs.Size) return false;
            
            // Optimisation BitMaps
            if (rhs is Bitmap rBitmap)
            {
                for (var cc = 0; cc < map.Length; cc++)
                    if (map[cc] != rBitmap.map[cc])
                        return false;
                return true;
            }

            for (var x = 0; x < size.X; x++)
            for (var y = 0; y < size.Y; y++)
                if (this[x, y] != rhs[x, y])
                    return false;

            return true;
        }

        /// <summary>
        ///     From String
        /// </summary>
        /// <param name="stringMap">map[], all others TRUE</param>
        /// <param name="where">On/Off function</param>
        public static Bitmap Create(string[] stringMap, Func<char, bool>? where = null)
        {
            if (where == null) where = x => x != ' ';
            var res = new Bitmap(stringMap.Max(x => x.Length), stringMap.Length);
            for (var yy = 0; yy < stringMap.Length; yy++)
            for (var xx = 0; xx < stringMap[yy].Length; xx++)
                res[xx, yy] = where(stringMap[yy][xx]);
            return res;
        }

        public static Bitmap Create(string[] stringMap, params char[] any) => Create(stringMap, any.Contains);

        public static VectorInt2 FindPosition(string[] textPuzzle, char c)
        {
            for (var yy = 0; yy < textPuzzle.Length; yy++)
            for (var xx = 0; xx < textPuzzle[yy].Length; xx++)
                if (textPuzzle[yy][xx] == c) return new VectorInt2(xx, yy);
            throw new Exception("Not Found");
        }


        public static Bitmap Create(string stringWithLineFeed, Func<char, bool>? where = null)
        {
            stringWithLineFeed = stringWithLineFeed.Replace("\n\r", "\n");
            return Create(stringWithLineFeed.Split('\n'), where);
        }


        public static Bitmap Create(VectorInt2 size, IEnumerable<VectorInt2> truePositions)
        {
            var res = new Bitmap(size);
            foreach (var t in truePositions) res[t] = true;
            return res;
        }
       
        public override bool Equals(object obj)
        {
            return Equals((IBitmap) obj);
        }


        public override int GetHashCode()
        {
            uint result = 0;
            for (uint ccy = 0; ccy < map.Length; ccy++)
                result = result + map[ccy] / (uint) 1.2345 * ccy;

            return (int) result;
        }

        public int HashUsingWeights(uint[] weights)
        {
            unchecked
            {
                uint result = 1;
                for (var y = 0; y < size.Y; y++)
                {
                    if (map[y] == 0) continue;
                    result += (map[y] * weights[y]);
                }
                return (int)result;
            }
        }


        public static bool operator ==(Bitmap lhs, Bitmap rhs)
        {
            if ((object) lhs == null && (object) rhs == null) return true;
            if ((object) lhs == null || (object) rhs == null) return false;
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Bitmap lhs, Bitmap rhs) => !(lhs == rhs);
        public static Bitmap operator |(Bitmap lhs, Bitmap rhs) => lhs.BitwiseOR(rhs);

        public override string ToString()
        {
            var rep = new StringBuilder();
            for (var ccy = 0; ccy < map.Length; ccy++)
            {
                for (var ccx = 0; ccx < size.X; ccx++)
                    if (this[ccx, ccy]) rep.Append('X');
                    else rep.Append('.');
                rep.Append(Environment.NewLine);
            }

            return rep.ToString();
        }
        
        public IEnumerable<bool> ForEachValue()
        {
            for (var yy = 0; yy < Height; yy++)
                for (var xx = 0; xx < Width; xx++)
                    yield return this[xx, yy];
        }
        
        public IEnumerator<(VectorInt2 Position, bool Value)> GetEnumerator()
        {
            for (var yy = 0; yy < Height; yy++)
                for (var xx = 0; xx < Width; xx++)
                    yield return (new VectorInt2(xx, yy), this[xx, yy]);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public string ToStringVerbose()
        {
            var rep = new StringBuilder();
            rep.Append(ToString());
            rep.AppendFormat("[{0}]", GetHashCode());
            return rep.ToString();
        }

      
    }
}