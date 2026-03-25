using VectorInt;

namespace SokoSolve.Primitives;

public class BitmapMaskedState
{
    private BitmapMaskedState(IReadOnlyBitmap mask, VectorInt2[] indexToPosition, int[,] positionToIndex)
    {
        this.Mask = mask;
        this.IndexToPosition = indexToPosition;
        this.PositionToIndex = positionToIndex;
    }

    public IReadOnlyBitmap Mask { get; }
    public VectorInt2[] IndexToPosition { get; }
    public int[,] PositionToIndex { get; }
    public int Width => Mask.Width;
    public int Height => Mask.Height;

    public static BitmapMaskedState Create(IReadOnlyBitmap mask)
    {
        var i2p = new VectorInt2[mask.Count];
        var p2i = new int[mask.Width, mask.Height];
        BitmapMasked.Fill(p2i, -1);
        var cc = 0;
        foreach(var p in mask.ForEachTruePosition())
        {
            i2p[cc] =  p;
            p2i[p.X, p.Y] = cc;
            cc++;
        }
        return new BitmapMaskedState(mask, i2p, p2i);
    }
}

public ref struct BitmapMaskedSpan
{
    readonly BitmapMaskedState state;
    readonly Span<byte> buffer;

    public BitmapMaskedSpan(BitmapMaskedState state, Span<byte> buffer)
    {
        this.state = state;
        this.buffer = buffer;
    }

    public bool this[int x, int y]
    {
        get
        {
            var idx = state.PositionToIndex[x,y];
            if (idx < 0) return false;
            return BitArrayHelper.GetBit(buffer, idx);
        }
        set
        {
            var idx = state.PositionToIndex[x,y];
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


    public void WriteTo(IBitmap map)
    {
        for(var y=0; y<state.Height; y++)
        {
            for(var x=0; x<state.Width; x++)
            {
                map[x,y] = this[x,y];
            }
        }
    }

    public void SetFrom(BitmapMaskedSpan map)
    {
        map.buffer.CopyTo(this.buffer);
    }

    public void SetFrom(IReadOnlyBitmap map)
    {
        foreach(var p in map.ForEach())
        {
            this[p.Position.X, p.Position.Y] = p.Value;
        }
    }

    public bool IsBitwiseANDMatch(IReadOnlyBitmap goalMap)
    {
        foreach(var t in goalMap.ForEachTruePosition())
        {
            if (!this[t.X, t.Y]) return false;
        }
        return true;
    }
}

public class BitmapMasked : IBitmap
{
    readonly IReadOnlyBitmap mask;
    readonly VectorInt2[] indexToPosition;
    readonly int[,] positionToIndex;
    readonly byte[] buffer;

    private BitmapMasked(BitmapMaskedState state)
    {
        this.mask = state.Mask;
        this.indexToPosition = state.IndexToPosition;
        this.positionToIndex = state.PositionToIndex;
        buffer = new byte[BitArrayHelper.CalcBytes(indexToPosition.Length)];
    }

    public static BitmapMasked Create(IReadOnlyBitmap mask)
    {
        return new BitmapMasked(BitmapMaskedState.Create(mask));
    }

    public static void Fill(int[,] arr, int val)
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

