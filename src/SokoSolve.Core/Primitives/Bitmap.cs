using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using VectorInt;

namespace SokoSolve.Core.Primitives
{
    public class Bitmap : IBitmap
    {
        private readonly uint[] map;
        private readonly VectorInt2 size;
        
        public Bitmap(VectorInt2 size) 
        {
            Debug.Assert(size.X <= 32);
            
            this.size = size;
            map  = new uint[size.Y];
        }
        
        public Bitmap(int aSizeX, int aSizeY) : this(new VectorInt2(aSizeX, aSizeY)) {}

        public Bitmap(IBitmap copy) : this(copy.Size.X, copy.Size.Y)
        {
            for (var cy = 0; cy < copy.Size.Y; cy++)
            for (var cx = 0; cx < copy.Size.X; cx++)
                this[cx, cy] = copy[cx, cy];
        }
        
        public Bitmap(Bitmap copy)
        {
            size = copy.size;
            map = new uint[copy.map.Length];
            copy.map.CopyTo(map, 0);
        }

        public static int SizeInBytes(VectorInt2 size) => size.Y * 4;

        public int        Width  => size.X;
        public int        Height => size.Y;
        public VectorInt2 Size   => size;
        
        public int Count
        {
            get
            {
                #if NET5_0
                var result = 0;
                for (var ccy = 0; ccy < map.Length; ccy++)
                {
                    result += System.Runtime.Intrinsics.X86.Popcnt.PopCount(map[cyy]);
                }
                return result;
#else
                

                var result = 0;
                for (var ccy = 0; ccy < map.Length; ccy++)
                {
                    if (map[ccy] == 0) continue;
                    for (var ccx = 0; ccx < size.X; ccx++)
                        if (this[ccx, ccy])
                            result++;
                }
                return result;
#endif
            }
        }

        public int SizeInBytes() => map.Length * sizeof(uint);
        
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
        

        public int CompareTo(IBitmap other)
        {
            if (other is Bitmap b)
            {
                for (var cy = 0; cy < size.Y; cy++)
                {
                    var c =  this.map[cy].CompareTo(b.map[cy]);
                    if (c != 0) return c;
                }
                return 0;
            }
            else
            {
                return BitmapHelper.Compare(this, other);
            }
        }

        public bool Equals(IBitmap? rhs)
        {
            if (rhs is null) return false;
            if (size != rhs.Size) return false;
            
            // Optimisation BitMaps
            if (rhs is Bitmap rBitmap)
            {
                for (var cc = 0; cc < map.Length; cc++)
                    if (map[cc] != rBitmap.map[cc])
                        return false;
                return true;
            }

            return BitmapHelper.Equal(this, rhs);
        }

        public Bitmap BitwiseOR(Bitmap rhs)
        {
            Debug.Assert(Size == rhs.Size);

            var res = new Bitmap(rhs.Size);
            for (var cy = 0; cy < rhs.Size.Y; cy++)
                res.map[cy] = this.map[cy] | rhs.map[cy];

            return res;
        }
        
        public Bitmap BitwiseAND(Bitmap rhs)
        {
            Debug.Assert(Size == rhs.Size);

            var res = new Bitmap(rhs.Size);
            for (var cy = 0; cy < rhs.Size.Y; cy++)
                res.map[cy] = this.map[cy] & rhs.map[cy];

            return res;
        }
     
       
        public override bool Equals(object obj) => Equals((IBitmap) obj);

        public static readonly BitmapHashOld BitmapHashOld = new BitmapHashOld();
        public int GetHashCodeOld() => BitmapHashOld.GetHashCode(map);

        public static readonly BitmapHashWeighted BitmapHashWeighted = new BitmapHashWeighted(Primes.List);
        public static readonly BitmapHashBigInteger BitmapHashBigInteger = new BitmapHashBigInteger();
        public static readonly BitmapHashCode BitmapHashCode = new BitmapHashCode();
        public override int GetHashCode() =>  BitmapHashCode.GetHashCode(map);
        
        public static bool   operator !=(Bitmap lhs, Bitmap rhs) => !(lhs == rhs);
        public static Bitmap operator |(Bitmap lhs, Bitmap rhs) => lhs.BitwiseOR(rhs);
        public static bool   operator ==(Bitmap? lhs, Bitmap? rhs)
        {
            if (lhs is null && rhs is null) return true;
            if (lhs is null || rhs is null) return false;
            return lhs.Equals(rhs);
        }

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
        
        public IEnumerable<VectorInt2> ForEachTrueValue()
        {
            for (var yy = 0; yy < Height; yy++)
            for (var xx = 0; xx < Width; xx++)
                if (this[xx, yy]) yield return new VectorInt2(xx, yy);
        }
        
        public IEnumerable<(VectorInt2, bool)> ForEach()
        {
            for (var yy = 0; yy < Height; yy++)
                for (var xx = 0; xx < Width; xx++)
                    yield return (new VectorInt2(xx, yy), this[xx, yy]);
        }
        
        // Just we I can compare to other IBitmap (without all the statics
        #region StaticFunctions

        /// <summary>
        /// From String
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
            stringWithLineFeed = stringWithLineFeed.Replace("\r", "");
            return Create(stringWithLineFeed.Split('\n'), where);
        }

        public static Bitmap Create(VectorInt2 size, IEnumerable<VectorInt2> truePositions)
        {
            var res = new Bitmap(size);
            foreach (var t in truePositions)
            {
                res[t] = true;
            }
            return res;
        }

        #endregion

        public ref uint GetPointer()
        {
            return ref map[0];
        }
    }
}