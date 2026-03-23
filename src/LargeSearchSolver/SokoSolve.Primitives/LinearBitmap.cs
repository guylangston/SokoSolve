using System.Runtime.CompilerServices;

namespace SokoSolve.Primitives;

public class LinearBitmap : IBitmap
{
    readonly byte[] buffer;
    readonly int width;
    readonly int height;

    public LinearBitmap(int width, int height)
    {
        if (width <= 0) throw new ArgumentException(null, nameof(width));
        if (height <= 0) throw new ArgumentException(null, nameof(height));

        this.width = width;
        this.height = height;

        var sizeBits = width * height;
        buffer = new byte[BitArrayHelper.CalcBytes((uint)sizeBits)];
    }

    public int Width => width;
    public int Height => height;

    public bool this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
#if DEBUG
            if (x < 0 || x >= width) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= height) throw new IndexOutOfRangeException(nameof(y));
#endif
            var bitIdx = (y * width) + x;
            return BitArrayHelper.GetBit(buffer, bitIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
#if DEBUG
            if (x < 0 || x >= width) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= height) throw new IndexOutOfRangeException(nameof(y));
#endif
            var bitIdx = (y * width) + x;
            BitArrayHelper.SetBit(buffer, bitIdx, value);
        }
    }
    public bool this[byte x, byte y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
#if DEBUG
            if (x >= width) throw new IndexOutOfRangeException(nameof(x));
            if (y >= height) throw new IndexOutOfRangeException(nameof(y));
#endif
            var bitIdx = (y * width) + x;
            return BitArrayHelper.GetBit(buffer, bitIdx);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
#if DEBUG
            if (x >= width) throw new IndexOutOfRangeException(nameof(x));
            if (y >= height) throw new IndexOutOfRangeException(nameof(y));
#endif
            var bitIdx = (y * width) + x;
            BitArrayHelper.SetBit(buffer, bitIdx, value);
        }
    }

    public int Count
    {
        get
        {
            var cc = 0;
            for(int y=0; y<height; y++)
            {
                for (int x=0; x<width; x++)
                {
                    cc += this[x,y] ? 1 : 0;
                }
            }
            return cc;
        }
    }


    public int CompareTo(IBitmap? other)
    {
        throw new NotImplementedException();
    }

    public bool Equals(IBitmap? other)
    {
        throw new NotImplementedException();
    }

    public int SizeInBytes() => buffer.Length;
}
