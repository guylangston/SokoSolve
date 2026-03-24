using VectorInt;

namespace SokoSolve.Primitives;

public class BitmapMasked : IBitmap
{
    readonly IReadOnlyBitmap mask;
    readonly VectorInt2[] indexToPosition;
    readonly int[,] positionToIndex;
    readonly byte[] buffer;

    private BitmapMasked(IReadOnlyBitmap mask, VectorInt2[] indexToPosition, int[,] positionToIndex)
    {
        this.mask = mask;
        this.indexToPosition = indexToPosition;
        this.positionToIndex = positionToIndex;
        buffer = new byte[BitArrayHelper.CalcBytes(indexToPosition.Length)];
    }

    public static BitmapMasked Create(IReadOnlyBitmap mask)
    {
        var i2p = new VectorInt2[mask.Count];
        var p2i = new int[mask.Width, mask.Height];
        Fill(p2i, -1);
        var cc = 0;
        foreach(var p in mask.ForEachTruePosition())
        {
            i2p[cc] =  p;
            p2i[p.X, p.Y] = cc;
            cc++;
        }
        return new BitmapMasked(mask, i2p, p2i);
    }

    private static void Fill(int[,] arr, int val)
    {
        for (int x = 0; x < arr.GetLength(0); x++)
        {
            for (int y = 0; y < arr.GetLength(1); y++)
            {
                arr[x, y] = val;
            }
        }
    }

    public bool this[int x, int y]
    {
        get
        {
            var idx = positionToIndex[x,y];
            if (idx < 0) return false;
            return BitArrayHelper.GetBit(buffer, idx);
        }
        set
        {
            var idx = positionToIndex[x,y];
            if (idx < 0)
            {
                if (!value) return;
                throw new ArgumentOutOfRangeException("x,y", $"({x},{y}) is immutable");
            }
            BitArrayHelper.SetBit(buffer, idx, value);
        }
    }

    public bool this[byte x, byte y]
    {
        get => this[(int)x, (int)y];
        set => this[(int)x, (int)y] = value;
    }

    public int Width => mask.Width;
    public int Height => mask.Height;
    public IReadOnlyBitmap Mask => mask;
    public int MutableCount => indexToPosition.Length;

    public int Count => BitArrayHelper.Count(buffer);

    public int CompareTo(IBitmap? other) => BitmapHelper.Compare(this, other);

    public bool Equals(IBitmap? other) => BitmapHelper.Equal(this, other);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.AddBytes(buffer);
        return hash.ToHashCode();
    }
    public override bool Equals(object? obj)
    {
        if (obj is IBitmap other)
        {
            return BitmapHelper.Equal(this, other);
        }
        return Object.Equals(this, obj);
    }

    public int SizeInBytes() => buffer.Length;

}

