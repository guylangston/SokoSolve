using VectorInt;

namespace SokoSolve.Primitives;

public interface IReadOnlyBitmap : IEquatable<IBitmap>, IComparable<IBitmap>
{
    int Width { get; }
    int Height { get; }
    bool this[int x, int y] { get; }
    bool this[byte x, byte y] { get; }
    int Count { get; }
    int SizeInBytes();
}

public interface IBitmap : IReadOnlyBitmap
{
    new bool this[int x, int y] { get; set; }
    new bool this[byte x, byte y] { get; set; }

    // TODO: Drop this?
    public bool this[VectorInt2 p]
    {
        get => this[p.X, p.Y];
        set => this[p.X, p.Y] = value;
    }
}

public static class BitmapExt
{
    extension(IBitmap map)
    {
        public VectorInt2 Size => new VectorInt2(map.Width, map.Height);
    }
}

