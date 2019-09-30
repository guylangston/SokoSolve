using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SokoSolve.Core.Primitives
{
    public interface IBitmap
    {
        bool this[VectorInt2 pos] { get; set; }
        bool this[int pX, int pY] { get; set; }
        VectorInt2 Size { get; set; }

        
    }

    public static class IBitmapExt
    {

        /// <summary>
        /// Does any cell intersect/overlap between lhs and rhs?
        /// </summary>
        public static bool Intersects(this IBitmap lhs, IBitmap rhs)
        {
            foreach (var t in lhs.TruePositions())
            {
                if (rhs[t]) return true;
            }
            return false;
        }

        public static IBitmap Invert(this IBitmap bitmap)
        {
            var res = new Bitmap(bitmap.Size);
            for (int cy = 0; cy < bitmap.Size.Y; cy++)
                for (int cx = 0; cx < bitmap.Size.X; cx++)
                {
                    res[cx, cy] = !bitmap[cx, cy];
                }
            return res;
        }

        public static IEnumerable<VectorInt2> TruePositions(this IBitmap bitmap)
        {
            for (int cy = 0; cy < bitmap.Size.Y; cy++)
                for (int cx = 0; cx < bitmap.Size.X; cx++)
                {
                    if (bitmap[cx, cy]) yield return new VectorInt2(cx, cy);
                }
        }

        public static IEnumerable<VectorInt2> FalsePositions(this IBitmap bitmap)
        {
            for (int cy = 0; cy < bitmap.Size.Y; cy++)
                for (int cx = 0; cx < bitmap.Size.X; cx++)
                {
                    if (!bitmap[cx, cy]) yield return new VectorInt2(cx, cy);
                }
        }

        public static Bitmap BitwiseOR(this IBitmap lhs, IBitmap rhs)
        {
            if (lhs.Size != rhs.Size) throw new InvalidDataException();

            // TODO: Optimise using uint
            var res = new Bitmap(lhs.Size);
            for (int cy = 0; cy < lhs.Size.Y; cy++)
                for (int cx = 0; cx < lhs.Size.X; cx++)
                {
                    res[cx, cy] = lhs[cx, cy] || rhs[cx, cy];
                }

            return res;
        }


        public static Bitmap BitwiseAND(this IBitmap lhs, IBitmap rhs)
        {
            if (lhs.Size != rhs.Size) throw new InvalidDataException();

            // TODO: Optimise using uint
            var res = new Bitmap(lhs.Size);
            for (int cy = 0; cy < lhs.Size.Y; cy++)
                for (int cx = 0; cx < lhs.Size.X; cx++)
                {
                    res[cx, cy] = lhs[cx, cy] && rhs[cx, cy];
                }

            return res;
        }

        public static IBitmap Subtract(this IBitmap lhs, IBitmap rhs)
        {
            if (lhs.Size != rhs.Size) throw new InvalidDataException();
            
            var res = new Bitmap(lhs);
            foreach (var on in rhs.TruePositions())
            {
                res[on] = false;
            }

            return res;
        }

        public static bool Contains(this IBitmap bitmap, VectorInt2 pos)
        {
            if (pos.X < 0 || pos.Y < 0) return false;
            if (pos.X >= bitmap.Size.X || pos.Y >= bitmap.Size.Y) return false;
            return true;
        }

        public static int Count(this IBitmap bitmap)
        {
            var b = bitmap as Bitmap;
            if (b != null) return b.Count;

            return bitmap.TruePositions().Count();
        }


        public static Map<char> ToCharMap(this IBitmap bitmap, char on = 'X', char off = '.')
        {
            var map = new Map<char>(bitmap.Size);
            map.Fill(off);
            foreach (var g in bitmap.TruePositions())
            {
                map[g] = on;
            }
            return map;
        }

        public static Map<int> ToIntMap(this IBitmap bitmap, int on = 1, int off = 0)
        {
            var map = new Map<int>(bitmap.Size);
            map.Fill(off);
            foreach (var g in bitmap.TruePositions())
            {
                map[g] = on;
            }
            return map;
        }

        public static Map<T> ToMap<T>(this IBitmap bitmap, T on, T off)
        {
            var map = new Map<T>(bitmap.Size);
            map.Fill(off);
            foreach (var g in bitmap.TruePositions())
            {
                map[g] = on;
            }
            return map;
        }
    }



    public class Bitmap : IBitmap, IEquatable<IBitmap>, IComparable<IBitmap>
    {
        private readonly uint[] map;
        private VectorInt2 size;
        
        public Bitmap(int aSizeX, int aSizeY)
        {
            if (aSizeX > 32) throw new NotSupportedException("Only 32bit sizes are excepted");
            size = new VectorInt2(aSizeX, aSizeY);
            map = new uint[aSizeY];
        }
        
        public Bitmap(VectorInt2 aSize) : this(aSize.X, aSize.Y){}

        /// <summary>
        /// Copy Constructor. Deep copy.
        /// </summary>
        public Bitmap(IBitmap copy)
            : this(copy.Size.X, copy.Size.Y)
        {
            for (int cy = 0; cy < copy.Size.Y; cy++)
                for (int cx = 0; cx < copy.Size.X; cx++)
                {
                    this[cx, cy] = copy[cx, cy];
                }
        }

        /// <summary>
        /// Copy Constructor. Deep copy. Optimised
        /// </summary>
        public Bitmap(Bitmap copy)
        {
            size = copy.size;
            map = new uint[copy.map.Length];
            copy.map.CopyTo(map, 0);
        }

        /// <summary>
        /// From String
        /// </summary>
        /// <param name="stringMap">map[], all others TRUE</param>
        /// <param name="where">On/Off function</param>
        public static Bitmap Create(string[] stringMap, Func<char, bool> where = null )
        {
            if (where == null) where = x => x != ' ';
            var res = new Bitmap(stringMap.Max(x => x.Length), stringMap.Length);
            for (int yy=0; yy<stringMap.Length; yy++)
                for (int xx = 0; xx < stringMap[yy].Length; xx++)
                {
                    res[xx, yy] = where(stringMap[yy][xx]);
                }
            return res;
        }



        public static Bitmap Create(string stringWithLineFeed, Func<char, bool> where = null)
        {
            stringWithLineFeed = stringWithLineFeed.Replace("\n\r", "\n");
            return Create(stringWithLineFeed.Split('\n'), where);       
        }


        public static Bitmap Create(VectorInt2 size, IEnumerable<VectorInt2> truePositions )
        {
            var res = new Bitmap(size);
            foreach (var t in truePositions)
            {
                res[t] = true;
            }
            return res;
        }

        
        public bool this[int pX, int pY]
        {
            get { return (map[pY] & (1 << pX)) > 0; }
            set
            {
                if (value)
                {
                    map[pY] = map[pY] | (uint)(1 << pX);
                }
                else
                {
                    map[pY] = map[pY] & ~(uint)(1 << pX);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTrue(int pX, int pY)
        {
            return (map[pY] & (1 << pX)) > 0; 
        }
    
        public bool this[VectorInt2 aPoint]
        {
            get { return this[aPoint.X, aPoint.Y]; }
            set { this[aPoint.X, aPoint.Y] = value; }
        }


        public VectorInt2 Size
        {
            get { return size; }
            set { throw new NotImplementedException(); }
        }

        
        /// <summary>
        /// The number of 1's (set bits)
        /// </summary>
        public int Count
        {
            get
            {
                int result = 0;
                for (int ccy = 0; ccy < map.Length; ccy++)
                {
                    if (map[ccy] == 0) continue;
                    for (int ccx = 0; ccx < size.X; ccx++)
                    {
                        if (IsTrue(ccx, ccy)) result++;
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Are any bits set? This is a fast function.
        /// </summary>
        public bool IsZero
        {
            get { return Count == 0; }
        }

        public bool Equals(IBitmap rhs)
        {
            // Optimisation BitMaps
            var rmap = rhs as Bitmap;
            if (rmap != null)
            {
                if (size != rmap.size) return false;
                for (int cc = 0; cc < map.Length; cc++)
                {
                    if (map[cc] != rmap.map[cc]) return false;
                }
                return true;
            }

            
            if (rhs != null)
            {
                if (size != rhs.Size) return false;
                for (int x = 0; x < size.X; x++)
                    for (int y = 0; y < size.Y; y++)
                        if (this[x, y] != rhs[x, y]) return false;

                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals((IBitmap) obj);
        }


        public override int GetHashCode()
        {
            uint result = 0;
            for (uint ccy = 0; ccy < map.Length; ccy++)
                result = result + (map[ccy] / (uint)1.2345 * ccy);

            return (int)result;
        }

        public int GetHashCodeUsingPositionWeights(int[] weights)
        {
            int result = 0;
            for (int y = 0; y < size.Y; y++)
            {
                if (map[y] == 0) continue;  // optimisation

                for (int x = 0; x < size.X; x++)    
                {
                    if (IsTrue(x, y)) result += weights[x + y * size.X];
                }
                    
            }
                

            return result;
        }

    
        public static bool operator ==(Bitmap lhs, Bitmap rhs)
        {
            if ((object)lhs == null && (object)rhs == null) return true;
            if ((object)lhs == null || (object)rhs == null) return false;
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Bitmap lhs, Bitmap rhs)
        {
            return !(lhs == rhs);
        }


        /// <summary>
        /// Lesser use bitwize-OR operator
        /// </summary>
        public static Bitmap operator |(Bitmap lhs, Bitmap rhs)
        {
            return lhs.BitwiseOR(rhs);
        }


        public int CompareTo(IBitmap other)
        {
            for (int ccy = 0; ccy < map.Length; ccy++)
            {
                for (int ccx = 0; ccx < size.X; ccx++)
                {
                    var me = this[ccx, ccy];
                    var you = other[ccx, ccy];
                    if (me && !you) return 1;
                    if (!me && you) return -1;
                }
            }
            return 0;
        }

        public override string ToString()
        {
            var rep = new StringBuilder();
            for (int ccy = 0; ccy < map.Length; ccy++)
            {
                for (int ccx = 0; ccx < size.X; ccx++)
                {
                    if (this[ccx, ccy]) rep.Append('X');
                    else rep.Append('.');
                }
                rep.Append(Environment.NewLine);
            }
            return rep.ToString();
        }

        public string ToStringVerbose()
        {
            var rep = new StringBuilder();
            rep.Append(ToString());
            rep.AppendFormat("[{0}]", GetHashCode());
            return rep.ToString();
        }


    }
}
