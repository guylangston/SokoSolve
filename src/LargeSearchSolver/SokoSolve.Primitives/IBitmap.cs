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

    // TODO: Drop this?
    public bool this[VectorInt2 p]
    {
        get => this[p.X, p.Y];
    }
}

public interface IBitmap : IReadOnlyBitmap
{
    new bool this[int x, int y] { get; set; }
    new bool this[byte x, byte y] { get; set; }

    // TODO: Drop this?
    new public bool this[VectorInt2 p]
    {
        get => this[p.X, p.Y];
        set => this[p.X, p.Y] = value;
    }
}

public static class BitmapExt
{
    extension(IReadOnlyBitmap bitmap)
    {
        public VectorInt2 Size => new VectorInt2(bitmap.Width, bitmap.Height);

        public IEnumerable<VectorInt2> ForEachTruePosition()
        {
            for (var yy = 0; yy < bitmap.Height; yy++)
            {
                for (var xx = 0; xx < bitmap.Width; xx++)
                {
                    if (bitmap[xx, yy])
                    {
                        yield return new VectorInt2(xx, yy);
                    }
                }
            }
        }

        public IEnumerable<(VectorInt2, bool)> ForEach()
        {
            for (var yy = 0; yy < bitmap.Height; yy++)
            {
                for (var xx = 0; xx < bitmap.Width; xx++)
                {
                    yield return (new VectorInt2(xx, yy), bitmap[xx, yy]);
                }
            }
        }
    }


}

