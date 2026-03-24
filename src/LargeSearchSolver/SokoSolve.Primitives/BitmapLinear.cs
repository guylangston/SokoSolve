using System.Runtime.CompilerServices;

namespace SokoSolve.Primitives;


public class BitmapLinear : IBitmap
{
    readonly byte[] buffer;
    readonly int width;
    readonly int height;

    public BitmapLinear(int width, int height)
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
