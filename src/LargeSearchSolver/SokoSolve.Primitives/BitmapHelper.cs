using System.Diagnostics;
using SokoSolve.Primitives;
using VectorInt;

namespace SokoSolve.Primitives;

public static class BitmapHelper
{
    /// <summary>
    ///     Does any cell intersect/overlap between lhs and rhs?
    /// </summary>
    public static bool Intersects(this IBitmap lhs, IBitmap rhs)
    {
        foreach (var t in lhs.TruePositions())
        {
            if (rhs[t])
                return true;
        }

        return false;
    }

    public static Bitmap Clone(this IBitmap map)
    {
        var ret = new Bitmap(map.Size);
        map.CopyTo(ret);
        return ret;
    }

    public static Bitmap NewBitmapOfSize(this IBitmap map) => new Bitmap(map.Size);

    public static void CopyTo(this IReadOnlyBitmap lhs, IBitmap rhs)
    {
        if (lhs.Size != rhs.Size) throw new InvalidOperationException($"lhs.Size({lhs.Size}) != rhs.Size({rhs.Size})");
        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
            {
                rhs[cx, cy] = lhs[cx, cy];
            }
        }
    }
    public static void SetFrom(this IBitmap lhs, IReadOnlyBitmap rhs)
    {
        if (lhs.Size != rhs.Size) throw new InvalidOperationException($"lhs.Size({lhs.Size}) != rhs.Size({rhs.Size})");
        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
            {
                lhs[cx, cy] = rhs[cx, cy];
            }
        }
    }

    public static void Fill(this IBitmap lhs, bool fill)
    {
        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
            {
                lhs[cx, cy] = fill;
            }
        }
    }

    public static IBitmap Invert(this IBitmap bitmap)
    {
        var res = new Bitmap(bitmap.Size);
        for (var cy = 0; cy < bitmap.Size.Y; cy++)
        {
            for (var cx = 0; cx < bitmap.Size.X; cx++)
                res[cx, cy] = !bitmap[cx, cy];
        }

        return res;
    }

    public static IEnumerable<VectorInt2> TruePositions(this IBitmap bitmap)
    {
        for (var cy = 0; cy < bitmap.Size.Y; cy++)
        {
            for (var cx = 0; cx < bitmap.Size.X; cx++)
            {
                if (bitmap[cx, cy])
                    yield return new VectorInt2(cx, cy);
            }
        }
    }

    public static IEnumerable<VectorInt2> FalsePositions(this IBitmap bitmap)
    {
        for (var cy = 0; cy < bitmap.Size.Y; cy++)
        {
            for (var cx = 0; cx < bitmap.Size.X; cx++)
            {
                if (!bitmap[cx, cy])
                    yield return new VectorInt2(cx, cy);
            }
        }
    }

    public static void SetBitwiseOR(this IBitmap res,  IBitmap lhs, IBitmap rhs)
    {
        Debug.Assert(lhs.Size == rhs.Size);

        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
                res[cx, cy] = lhs[cx, cy] || rhs[cx, cy];
        }
    }

    public static void SetBitwiseAND(this IBitmap res, IBitmap lhs, IBitmap rhs)
    {
        Debug.Assert(lhs.Size == rhs.Size);

        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
                res[cx, cy] = lhs[cx, cy]  && rhs[cx, cy];
        }
    }

    public static Bitmap BitwiseOR(this IBitmap lhs, IBitmap rhs)
    {
        if (lhs is Bitmap lb && rhs is Bitmap rb) return lb.BitwiseOR(rb);

        Debug.Assert(lhs.Size == rhs.Size);
        var res = new Bitmap(lhs.Size);
        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
                res[cx, cy] = lhs[cx, cy] || rhs[cx, cy];
        }

        return res;
    }

    public static Bitmap BitwiseAND(this IBitmap lhs, IBitmap rhs)
    {
        if (lhs is Bitmap lb && rhs is Bitmap rb) return lb.BitwiseAND(rb);

        Debug.Assert(lhs.Size == rhs.Size);
        var res = new Bitmap(lhs.Size);
        for (var cy = 0; cy < lhs.Size.Y; cy++)
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
                res[cx, cy] = lhs[cx, cy] && rhs[cx, cy];
        }

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

    public static bool WithinBounds(this IBitmap bitmap, VectorInt2 pos)
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

    public static bool Equal(IBitmap? a, IBitmap? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Size != b.Size) return false;

        for (var y = 0; y < a.Size.Y; y++)
        {
            for (var x = 0; x < a.Size.X; x++)
                if (a[x, y] != b[x, y])  return false;
        }

        return true;
    }

    public static int Compare(IBitmap? a, IBitmap? b)
    {
        if (a is null && b is null) return 0;
        if (a is null) return -1;
        if (b is null) return 1;

        for (var y = 0; y < a.Size.Y; y++)
        {
            for (var x = 0; x < a.Size.X; x++)
            {
                var aa = a[x, y];
                var bb = b[x, y];
                if (aa && !bb) return 1;
                if (!aa && bb) return -1;
            }
        }
        return 0;
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
        {
            for (var cx = 0; cx < lhs.Size.X; cx++)
                cc += (lhs[cx, cy] & rhs[cx, cy])  ? 1: 0;
        }

        return cc;
    }

    /// <summary>
    /// From StringArray
    /// </summary>
    /// <param name="where">On/Off function</param>
    public static T CreateFromStrings<T>(string stringMapMultiLine, Func<char, bool>? where = null, Func<int, int, T>? factoryBitmap = null)  where T:IBitmap
    {
        var reader = new StringReader(stringMapMultiLine);
        var lines = new List<string>();
        while (reader.ReadLine() is {} ln)
        {
            lines.Add(ln);
        }
        return CreateFromStrings(lines, where, factoryBitmap);
    }


    /// <summary>
    /// From StringArray
    /// </summary>
    /// <param name="stringMap">map[], all others TRUE</param>
    /// <param name="where">On/Off function</param>
    public static T CreateFromStrings<T>(IReadOnlyList<string> stringMap, Func<char, bool>? where = null, Func<int, int, T>? factoryBitmap = null)  where T:IBitmap
    {
        if (where == null)
        {
            where = x => (x != ' ') && (x != '.');
        }
        if (factoryBitmap == null)
        {
            factoryBitmap = FactoryBitmapDefault;
        }

        var res = factoryBitmap(stringMap.Max(x => x.Length), stringMap.Count);
        for (var yy = 0; yy < stringMap.Count; yy++)
        {
            for (var xx = 0; xx < stringMap[yy].Length; xx++)
            {
                res[xx, yy] = where(stringMap[yy][xx]);
            }
        }

        return res;

        static T FactoryBitmapDefault(int x, int y)
        {
            IBitmap b = new Bitmap(x, y);
            return (T)b;
        }
    }

    public static Bitmap CreateFromTruePositions(VectorInt2 size, IEnumerable<VectorInt2> truePositions)
    {
        var res = new Bitmap(size);
        foreach (var t in truePositions)
        {
            res[t] = true;
        }
        return res;
    }

    public static byte[] ToBinarySequenceInBytes(IBitmap map)
    {
        var sizeBits = map.Width * map.Height;
        var sizeBytes = (sizeBits / 8) + (sizeBits % 8 > 0 ? 1: 0);
        var res = new byte[sizeBytes];

        var idx =0;
        for(int y=0; y<map.Height; y++)
        {
            for(int x=0; x<map.Width; x++)
            {
                var bit = map[x,y] ? 1 : 0;
                var byteIdx = idx / 8;
                var bitIdx = idx % 8;
                res[byteIdx] |= (byte)(bit << bitIdx);
                idx++;
            }
        }

        return res;
    }
}
