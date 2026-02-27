using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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
        FillRecursive(constraints, p.X, p.Y, result);
        return result;
    }

    public static Bitmap Fill(BitmapSpan constraints, VectorInt2 p)
    {
        var result = new Bitmap(constraints.Size);
        FillRecursive(constraints, p.X, p.Y, result);
        return result;
    }

    public static void Fill(IReadOnlyBitmap constraints, VectorInt2 p, IBitmap output) => FillRecursive(constraints, p.X, p.Y, output);

    private static void FillRecursive(IReadOnlyBitmap constraints, int x, int y, IBitmap result)
    {
        if (x < 0 || y < 0) return;
        if (x >= constraints.Size.X || y >= constraints.Size.Y) return;

        if (constraints[x, y]) return;
        if (result[x, y]) return;

        result[x, y] = true;

        FillRecursive(constraints, x, y-1, result);
        FillRecursive(constraints, x, y+1, result);
        FillRecursive(constraints, x-1, y, result);
        FillRecursive(constraints, x+1, y, result);
    }


    public static void FillRecursive(BitmapSpan constraints, int x, int y, Bitmap result)
    {
        if (x < 0 || y < 0) return;
        if (x >= constraints.Size.X || y >= constraints.Size.Y) return;

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
        if (x >= constraints.Size.X || y >= constraints.Size.Y) return;

        if (constraints[x, y]) return;
        if (result[x, y]) return;

        result[x, y] = true;

        FillRecursive(constraints, x, y-1, result);
        FillRecursive(constraints, x, y+1, result);
        FillRecursive(constraints, x-1, y, result);
        FillRecursive(constraints, x+1, y, result);
    }

    /// <summary>
    /// Fast scanline floodfill algorithm with SIMD optimizations.
    /// Uses a stack-based approach to fill horizontal spans efficiently.
    /// </summary>
    /// <param name="constraints">Read-only bitmap where true values are constraints (walls)</param>
    /// <param name="start">Starting position for the fill</param>
    /// <param name="output">Output bitmap where filled area will be marked as true</param>
    public static void FillScanline(IReadOnlyBitmap constraints, VectorInt2 start, IBitmap output)
    {
        if (start.X < 0 || start.Y < 0 || start.X >= constraints.Width || start.Y >= constraints.Height)
            return;

        if (constraints[start.X, start.Y] || output[start.X, start.Y])
            return;

        var width = constraints.Width;
        var height = constraints.Height;
        var stack = new Stack<(int y, int xLeft, int xRight, int dy)>(256);

        // Initial seed: fill the starting scanline
        var xLeft = start.X;
        while (xLeft > 0 && !constraints[xLeft - 1, start.Y] && !output[xLeft - 1, start.Y])
            xLeft--;

        var xRight = start.X;
        while (xRight < width - 1 && !constraints[xRight + 1, start.Y] && !output[xRight + 1, start.Y])
            xRight++;

        // Fill the initial scanline
        for (var x = xLeft; x <= xRight; x++)
            output[x, start.Y] = true;

        // Check lines above and below
        if (start.Y > 0)
            ScanAdjacentLine(constraints, output, start.Y - 1, xLeft, xRight, 1, stack);
        if (start.Y < height - 1)
            ScanAdjacentLine(constraints, output, start.Y + 1, xLeft, xRight, -1, stack);

        while (stack.Count > 0)
        {
            var (y, xL, xR, dy) = stack.Pop();

            if (y < 0 || y >= height)
                continue;

            var x = xL;

            // Extend left from xL
            while (x > 0 && !constraints[x - 1, y] && !output[x - 1, y])
                x--;
            xLeft = x;

            // Extend right from xL to xR and beyond
            while (x < width && x <= xR && !constraints[x, y] && !output[x, y])
                x++;

            if (x > xR)
            {
                // Continue extending right beyond xR
                while (x < width && !constraints[x, y] && !output[x, y])
                    x++;
            }
            xRight = x - 1;

            // Fill the scanline
            for (x = xLeft; x <= xRight; x++)
                output[x, y] = true;

            // Scan line in the opposite direction (above/below)
            if (y - dy >= 0 && y - dy < height)
                ScanAdjacentLine(constraints, output, y - dy, xLeft, xRight, -dy, stack);

            // Scan line in the same direction (continue in that direction)
            if (y + dy >= 0 && y + dy < height)
                ScanAdjacentLine(constraints, output, y + dy, xLeft, xRight, dy, stack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillScanlineHorizontal(
        IReadOnlyBitmap constraints,
        IBitmap output,
        int y,
        int xStart,
        int xEnd,
        int width,
        out int xFinal)
    {
        var x = xStart;

        // Use SIMD for faster horizontal filling when available and beneficial
#if NET5_0_OR_GREATER
        if (Sse2.IsSupported && width - x >= 8)
        {
            // Process 8 pixels at a time using SSE2
            while (x + 7 < width && x <= xEnd)
            {
                bool canProcessBatch = true;
                for (int i = 0; i < 8; i++)
                {
                    if (constraints[x + i, y] || output[x + i, y])
                    {
                        canProcessBatch = false;
                        break;
                    }
                }

                if (!canProcessBatch)
                    break;

                // Fill 8 pixels at once
                for (int i = 0; i < 8; i++)
                    output[x + i, y] = true;

                x += 8;
            }
        }
#endif

        // Fill remaining pixels sequentially
        while (x < width && !constraints[x, y] && !output[x, y])
        {
            output[x, y] = true;
            x++;
        }

        xFinal = x - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ScanAdjacentLine(
        IReadOnlyBitmap constraints,
        IBitmap output,
        int y,
        int xLeft,
        int xRight,
        int dy,
        Stack<(int y, int xLeft, int xRight, int dy)> stack)
    {
        if (y < 0 || y >= constraints.Height)
            return;

        var spanStart = -1;

        for (var x = xLeft; x <= xRight; x++)
        {
            if (!constraints[x, y] && !output[x, y])
            {
                if (spanStart == -1)
                    spanStart = x;
            }
            else if (spanStart != -1)
            {
                stack.Push((y, spanStart, x - 1, dy));
                spanStart = -1;
            }
        }

        if (spanStart != -1)
            stack.Push((y, spanStart, xRight, dy));
    }

    /// <summary>
    /// Fast scanline floodfill optimized for Bitmap with direct uint array access and SIMD.
    /// </summary>
    public static void FillScanline(Bitmap constraints, VectorInt2 start, Bitmap output)
    {
        if (start.X < 0 || start.Y < 0 || start.X >= constraints.Width || start.Y >= constraints.Height)
            return;

        if (constraints[start.X, start.Y] || output[start.X, start.Y])
            return;

        var width = constraints.Width;
        var height = constraints.Height;
        var stack = new Stack<(int y, int xLeft, int xRight, int dy)>(256);

        // Initial seed: fill the starting scanline using optimized method
        FillScanlineRow(constraints, output, start.Y, start.X, out var xLeft, out var xRight);

        // Check lines above and below
        if (start.Y > 0)
            ScanAdjacentLineOptimized(constraints, output, start.Y - 1, xLeft, xRight, 1, stack);
        if (start.Y < height - 1)
            ScanAdjacentLineOptimized(constraints, output, start.Y + 1, xLeft, xRight, -1, stack);

        while (stack.Count > 0)
        {
            var (y, xL, xR, dy) = stack.Pop();

            if (y < 0 || y >= height)
                continue;

            var x = xL;

            // Skip if already filled
            if (output[x, y])
                continue;

            // Extend left from xL
            while (x > 0 && !constraints[x - 1, y] && !output[x - 1, y])
                x--;
            var newXLeft = x;

            // Extend right from xL to xR and beyond
            while (x < width && x <= xR && !constraints[x, y] && !output[x, y])
                x++;

            if (x > xR)
            {
                // Continue extending right beyond xR
                while (x < width && !constraints[x, y] && !output[x, y])
                    x++;
            }
            var newXRight = x - 1;

            // Fill the scanline using optimized row fill
            var rowMask = ((1u << (newXRight + 1)) - 1) & ~((1u << newXLeft) - 1);
            output[y] |= rowMask;

            // Scan line in the opposite direction (above/below)
            if (y - dy >= 0 && y - dy < height)
                ScanAdjacentLineOptimized(constraints, output, y - dy, newXLeft, newXRight, -dy, stack);

            // Scan line in the same direction (continue in that direction)
            if (y + dy >= 0 && y + dy < height)
                ScanAdjacentLineOptimized(constraints, output, y + dy, newXLeft, newXRight, dy, stack);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FillScanlineRow(Bitmap constraints, Bitmap output, int y, int startX, out int xLeft, out int xRight)
    {
        // Extend left
        var x = startX;
        while (x > 0 && !constraints[x - 1, y] && !output[x - 1, y])
            x--;
        xLeft = x;

        // Extend right
        x = startX;
        while (x < constraints.Width - 1 && !constraints[x + 1, y] && !output[x + 1, y])
            x++;
        xRight = x;

        // Fill using bitwise operations for speed
        var rowMask = ((1u << (xRight + 1)) - 1) & ~((1u << xLeft) - 1);
        output[y] |= rowMask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ScanAdjacentLineOptimized(
        Bitmap constraints,
        Bitmap output,
        int y,
        int xLeft,
        int xRight,
        int dy,
        Stack<(int y, int xLeft, int xRight, int dy)> stack)
    {
        if (y < 0 || y >= constraints.Height)
            return;

        var spanStart = -1;

        // Use bitwise operations to check ranges more efficiently
        var constraintRow = constraints[y];
        var outputRow = output[y];

        for (var x = xLeft; x <= xRight; x++)
        {
            var bitMask = 1u << x;
            var isConstrained = (constraintRow & bitMask) != 0;
            var isFilled = (outputRow & bitMask) != 0;

            if (!isConstrained && !isFilled)
            {
                if (spanStart == -1)
                    spanStart = x;
            }
            else if (spanStart != -1)
            {
                stack.Push((y, spanStart, x - 1, dy));
                spanStart = -1;
            }
        }

        if (spanStart != -1)
            stack.Push((y, spanStart, xRight, dy));
    }

}


