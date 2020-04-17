using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VectorInt;

namespace SokoSolve.Core.Primitives
{
    public static class BitmapHelper
    {
        /// <summary>
        ///     Does any cell intersect/overlap between lhs and rhs?
        /// </summary>
        public static bool Intersects(this IBitmap lhs, IBitmap rhs)
        {
            foreach (var t in lhs.TruePositions())
                if (rhs[t])
                    return true;
            return false;
        }

        public static IBitmap Invert(this IBitmap bitmap)
        {
            var res = new Bitmap(bitmap.Size);
            for (var cy = 0; cy < bitmap.Size.Y; cy++)
            for (var cx = 0; cx < bitmap.Size.X; cx++)
                res[cx, cy] = !bitmap[cx, cy];
            return res;
        }

        public static IEnumerable<VectorInt2> TruePositions(this IBitmap bitmap)
        {
            for (var cy = 0; cy < bitmap.Size.Y; cy++)
            for (var cx = 0; cx < bitmap.Size.X; cx++)
                if (bitmap[cx, cy])
                    yield return new VectorInt2(cx, cy);
        }

        public static IEnumerable<VectorInt2> FalsePositions(this IBitmap bitmap)
        {
            for (var cy = 0; cy < bitmap.Size.Y; cy++)
            for (var cx = 0; cx < bitmap.Size.X; cx++)
                if (!bitmap[cx, cy])
                    yield return new VectorInt2(cx, cy);
        }

        
        public static Bitmap BitwiseOR(this IBitmap lhs, IBitmap rhs)
        {
            if (lhs is Bitmap lb && rhs is Bitmap rb) return lb.BitwiseOR(rb);
            
            Debug.Assert(lhs.Size == rhs.Size);
            var res = new Bitmap(lhs.Size);
            for (var cy = 0; cy < lhs.Size.Y; cy++)
            for (var cx = 0; cx < lhs.Size.X; cx++)
                res[cx, cy] = lhs[cx, cy] || rhs[cx, cy];

            return res;
        }


        public static Bitmap BitwiseAND(this IBitmap lhs, IBitmap rhs)
        {
            if (lhs is Bitmap lb && rhs is Bitmap rb) return lb.BitwiseAND(rb);
            
            Debug.Assert(lhs.Size == rhs.Size);
            var res = new Bitmap(lhs.Size);
            for (var cy = 0; cy < lhs.Size.Y; cy++)
            for (var cx = 0; cx < lhs.Size.X; cx++)
                res[cx, cy] = lhs[cx, cy] && rhs[cx, cy];

            return res;
        }

        public static IBitmap Subtract(this IBitmap lhs, IBitmap rhs)
        {
            if (lhs.Size != rhs.Size) throw new InvalidDataException();

            var res  = new Bitmap(lhs);
            foreach (var on in rhs.TruePositions()) 
                res[on] = false;

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
            if (bitmap is Bitmap b) return b.Count;
            return bitmap.TruePositions().Count();
        }

        public static bool Equal(IBitmap a, IBitmap b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            if (a.Size != b.Size) return false;
            
            for (var x = 0; x < a.Size.X; x++)
            for (var y = 0; y < a.Size.Y; y++)
                if (a[x, y] != b[x, y])
                    return false;

            return true;
        }
        
        /// <summary>
        /// WARNING: return=0 DOES NOT IMPLY Equal(a,b)==true!
        /// </summary>
        public static int Compare(IBitmap a, IBitmap b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            return a.GetHashCode().CompareTo(b.GetHashCode());
        }


        public static Map<char> ToCharMap(this IBitmap bitmap, char on = 'X', char off = '.')
        {
            var map = new Map<char>(bitmap.Size);
            map.Fill(off);
            foreach (var g in bitmap.TruePositions()) map[g] = on;
            return map;
        }

        public static Map<int> ToIntMap(this IBitmap bitmap, int on = 1, int off = 0)
        {
            var map = new Map<int>(bitmap.Size);
            map.Fill(off);
            foreach (var g in bitmap.TruePositions()) map[g] = on;
            return map;
        }

        public static Map<T> ToMap<T>(this IBitmap bitmap, T on, T off)
        {
            var map = new Map<T>(bitmap.Size);
            map.Fill(off);
            foreach (var g in bitmap.TruePositions()) map[g] = on;
            return map;
        }

        public static int CountAND(IBitmap lhs, IBitmap rhs)
        {
            var cc = 0;
            if (lhs is Bitmap aa && rhs is Bitmap bb)
            {
                for (var cy = 0; cy < lhs.Size.Y; cy++)
                {
                    var ll = aa[cy] & bb[cy];
                    if (ll == 0) continue;
                    
                    for (var cx = 0; cx < lhs.Size.X; cx++)
                    {
                        cc += (((1 << cx) & ll) > 0) ? 1 : 0;
                    }
                }
                return cc;
            }
            
            for (var cy = 0; cy < lhs.Size.Y; cy++)
            for (var cx = 0; cx < lhs.Size.X; cx++)
                cc += (lhs[cx, cy] & rhs[cx, cy])  ? 1: 0;
            return cc;
        }

    }
}