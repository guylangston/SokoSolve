using VectorInt;

namespace SokoSolve.Primitives.Analytics;

// TODO: Remove?
public interface IBitmapFloodFill
{
    void Fill(IReadOnlyBitmap constraints, VectorInt2 start, IBitmap output);
    void Fill(BitmapSpan constraints, VectorInt2 start, IBitmap output);
}

/*
 * http://www.adammil.net/blog/v126_A_More_Efficient_Flood_Fill.html
 *
 */
public static class FloodFill
{

    public static Bitmap Fill(IReadOnlyBitmap constraints, VectorInt2 p)
    {
        var result = new Bitmap(constraints.Size);
        FillRecursive(constraints, p, result);
        return result;
    }

    public static Bitmap Fill(BitmapSpan constraints, VectorInt2 p)
    {
        var result = new Bitmap(constraints.size);
        FillRecursive(constraints, p.X, p.Y, result);
        return result;
    }

    public static void Fill(IReadOnlyBitmap constraints, VectorInt2 p, IBitmap output) => FillRecursive(constraints, p, output);

    private static void FillRecursive(IReadOnlyBitmap constraints, VectorInt2 p, IBitmap result)
    {
        if (p.X < 0 || p.Y < 0) return;
        if (p.X > constraints.Size.X || p.Y > constraints.Size.Y) return;

        if (constraints[p]) return;
        if (result[p]) return;

        result[p] = true;

        FillRecursive(constraints, p + VectorInt2.Up, result);
        FillRecursive(constraints, p + VectorInt2.Down, result);
        FillRecursive(constraints, p + VectorInt2.Left, result);
        FillRecursive(constraints, p + VectorInt2.Right, result);
    }


    public static void FillRecursive(BitmapSpan constraints, int x, int y, Bitmap result)
    {
        if (x < 0 || y < 0) return;
        if (x >= constraints.size.X || y >= constraints.size.Y) return;

        if (constraints[x, y]) return;
        if (result[x, y]) return;

        result[x, y] = true;

        FillRecursive(constraints, x, y-1, result);
        FillRecursive(constraints, x, y+1, result);
        FillRecursive(constraints, x-1, y, result);
        FillRecursive(constraints, x+1, y, result);
    }

    public static void FillRecursive(BitmapSpan constraints, int x, int y, BitmapSpan result)
    {
        if (x < 0 || y < 0) return;
        if (x >= constraints.size.X || y >= constraints.size.Y) return;

        if (constraints[x, y]) return;
        if (result[x, y]) return;

        result[x, y] = true;

        FillRecursive(constraints, x, y-1, result);
        FillRecursive(constraints, x, y+1, result);
        FillRecursive(constraints, x-1, y, result);
        FillRecursive(constraints, x+1, y, result);
    }

}


