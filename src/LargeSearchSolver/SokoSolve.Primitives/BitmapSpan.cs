using System;
using System.Runtime.CompilerServices;
using System.Text;
using VectorInt;

namespace SokoSolve.Primitives;

public readonly ref struct BitmapSpan // IBitmap
{
    private readonly Span<uint> map;
    public readonly VectorInt2 Size;

    public BitmapSpan(VectorInt2 size, Span<uint> map)
    {
        this.Size = size;
        this.map = map;
    }

    public int Width => Size.X;
    public int Height => Size.Y;

    public bool this[VectorInt2 aPoint]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[aPoint.X, aPoint.Y];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[aPoint.X, aPoint.Y] = value;
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

    public void Fill(bool val)
    {
        for (var cy = 0; cy < Size.Y; cy++)
        {
            for (var cx = 0; cx < Size.X; cx++)
                this[cx, cy] = val;
        }
    }

    public void Set(IBitmap source)
    {
        for (var cy = 0; cy < Size.Y; cy++)
        {
            for (var cx = 0; cx < Size.X; cx++)
                this[cx, cy] = source[cx, cy];
        }
    }

    public void CopyTo(IBitmap dest)
    {
        for (var cy = 0; cy < Size.Y; cy++)
        {
            for (var cx = 0; cx < Size.X; cx++)
                dest[cx, cy] = this[cx, cy];
        }
    }

    public void WriteText(TextWriter tw, char cTrue, char cFalse)
    {
        for (var cy = 0; cy < Size.Y; cy++)
        {
            for (var cx = 0; cx < Size.X; cx++)
            {
                tw.Write(this[cx, cy] ? cTrue : cFalse);
            }
            tw.WriteLine();
        }
    }

    public void SetBitwiseOR(BitmapSpan a, IReadOnlyBitmap b)
    {
        for (var cy = 0; cy < Size.Y; cy++)
        {
            for (var cx = 0; cx < Size.X; cx++)
                this[cx, cy] = a[cx, cy] || b[cx, cy];
        }
    }

    public void SetBitwiseOR(IReadOnlyBitmap a, IReadOnlyBitmap b)
    {
        for (var cy = 0; cy < Size.Y; cy++)
        {
            for (var cx = 0; cx < Size.X; cx++)
                this[cx, cy] = a[cx, cy] || b[cx, cy];
        }
    }

    public override string ToString()
    {
        var rep = new StringBuilder();
        for (var ccy = 0; ccy < Size.Y; ccy++)
        {
            for (var ccx = 0; ccx < Size.X; ccx++)
                rep.Append(this[ccx, ccy] ? 'X' : '.');
            rep.Append(Environment.NewLine);
        }

        return rep.ToString();
    }

}
