using System;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Analytics
{

    /*
     * http://www.adammil.net/blog/v126_A_More_Efficient_Flood_Fill.html
     *
     */
    public static class FloodFill
    {

        public static Bitmap Fill(IReadOnlyBitmap constraints, VectorInt2 p)
        {
            var result = new Bitmap(constraints.Size);
            FillCell(constraints, p, result);
            return result;
        }

        public static void Fill(IReadOnlyBitmap constraints, VectorInt2 p, IBitmap output) => FillCell(constraints, p, output);

        private static void FillCell(IReadOnlyBitmap constraints, VectorInt2 p, IBitmap result)
        {
            if (p.X < 0 || p.Y < 0) return;
            if (p.X > constraints.Size.X || p.Y > constraints.Size.Y) return;

            if (constraints[p]) return;
            if (result[p]) return;

            result[p] = true;

            FillCell(constraints, p + VectorInt2.Up, result);
            FillCell(constraints, p + VectorInt2.Down, result);
            FillCell(constraints, p + VectorInt2.Left, result);
            FillCell(constraints, p + VectorInt2.Right, result);
        }

        public static Bitmap Fill(BitmapSpan constraints, VectorInt2 p)
        {
            var result = new Bitmap(constraints.Size);
            Fill(constraints, p, result);
            return result;
        }

        public static void Fill(BitmapSpan constraints, VectorInt2 p, IBitmap output) => FillCell(constraints, p, output);

        private static void FillCell(BitmapSpan constraints, VectorInt2 p, IBitmap output)
        {
            if (p.X < 0 || p.Y < 0) return;
            if (p.X > constraints.Size.X || p.Y > constraints.Size.Y) return;

            if (constraints[p]) return;
            if (output[p]) return;

            output[p] = true;

            FillCell(constraints, p + VectorInt2.Up, output);
            FillCell(constraints, p + VectorInt2.Down, output);
            FillCell(constraints, p + VectorInt2.Left, output);
            FillCell(constraints, p + VectorInt2.Right, output);
        }

        public static void FillRecursiveOptimised(BitmapSpan constraints, int x, int y, Bitmap result)
        {
            if (x < 0 || y < 0) return;
            if (x > constraints.Size.X || y > constraints.Size.Y) return;

            if (constraints[x, y]) return;
            if (result[x, y]) return;

            result[x, y] = true;

            FillRecursiveOptimised(constraints, x, y-1, result);
            FillRecursiveOptimised(constraints, x, y+1, result);
            FillRecursiveOptimised(constraints, x-1, y, result);
            FillRecursiveOptimised(constraints, x+1, y, result);
        }

    }

    public interface IBitmapFloodFill
    {
        void Fill(IReadOnlyBitmap constraints, VectorInt2 start, IBitmap output);
        void Fill(BitmapSpan constraints, VectorInt2 start, IBitmap output);
    }

    public class BitmapFloodFillRecursive : IBitmapFloodFill
    {
        public void Fill(IReadOnlyBitmap constraints, VectorInt2 start, IBitmap output)
            => FloodFill.Fill(constraints, start, output);

        public void Fill(BitmapSpan constraints, VectorInt2 start, IBitmap output)
            => FloodFill.Fill(constraints, start, output);
    }

}
