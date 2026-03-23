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
        var sizeBytes = (sizeBits / 8) + (sizeBits % 8 > 0 ? 1 : 0);
        buffer = new byte[sizeBytes];
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
            var byteIdx = bitIdx / 8;
            var byteBit = bitIdx % 8;
            return (buffer[byteIdx] & (1 << byteBit)) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
#if DEBUG
            if (x < 0 || x >= width) throw new IndexOutOfRangeException(nameof(x));
            if (y < 0 || y >= height) throw new IndexOutOfRangeException(nameof(y));
#endif

            var bitIdx = (y * width) + x;
            var byteIdx = bitIdx / 8;
            var byteBit = bitIdx % 8;

            buffer[byteIdx] = value
                ? (byte)(buffer[byteIdx] |  (1 << byteBit))
                : (byte)(buffer[byteIdx] & ~(1 << byteBit));
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
            var byteIdx = bitIdx / 8;
            var byteBit = bitIdx % 8;
            return (buffer[byteIdx] & (1 << byteBit)) > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
#if DEBUG
            if (x >= width) throw new IndexOutOfRangeException(nameof(x));
            if (y >= height) throw new IndexOutOfRangeException(nameof(y));
#endif

            var bitIdx = (y * width) + x;
            var byteIdx = bitIdx / 8;
            var byteBit = bitIdx % 8;

            buffer[byteIdx] = value
                ? (byte)(buffer[byteIdx] |  (1 << byteBit))
                : (byte)(buffer[byteIdx] & ~(1 << byteBit));
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
