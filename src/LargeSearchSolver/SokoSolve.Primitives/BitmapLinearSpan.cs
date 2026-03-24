using System.Runtime.CompilerServices;

namespace SokoSolve.Primitives;

public ref struct BitmapLinearSpan // IBitmap
{
    readonly Span<byte> buffer;
    readonly int width;
    readonly int height;

    public BitmapLinearSpan(Span<byte> buffer, int width, int height)
    {
        this.width = width;
        this.height = height;

#if DEBUG
        if (width <= 0) throw new ArgumentException(null, nameof(width));
        if (height <= 0) throw new ArgumentException(null, nameof(height));
        var sizeBits = width * height;
        var sizeBytes = BitArrayHelper.CalcBytes((uint)sizeBits);
        if (buffer.Length < sizeBytes) throw new ArgumentException($"Buffer is too small. Required: {sizeBytes} bytes.", nameof(buffer));
#endif

        this.buffer = buffer;
    }

    public int Width => width;
    public int Height => height;

    public bool this[int x, int y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitArrayHelper.GetBit(buffer, width, height, x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => BitArrayHelper.SetBit(buffer, width, height, x, y, value);
    }

    public bool this[byte x, byte y]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitArrayHelper.GetBit(buffer, width, height, x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => BitArrayHelper.SetBit(buffer, width, height, x, y, value);
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
}

