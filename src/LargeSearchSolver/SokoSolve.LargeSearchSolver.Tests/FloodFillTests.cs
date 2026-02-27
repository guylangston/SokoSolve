using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using VectorInt;
namespace SokoSolve.LargeSearchSolver.Tests;

public class FloodFillTests
{
    public class TestArgs
    {
        public string Input   { get; set;} = 
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        public string Output  { get; set;} = 
            """
            ........
            .XXXXXX.
            .X....X.
            .XX..XX.
            .XXXXXX.
            ........

            """;
    }

    public TestArgs Args { get; set; } = new TestArgs();

    [Fact]
    public void FloodFill_Bitmap()
    {
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(Args.Input);
        var res = FloodFill.Fill(bitmap, (1,1));
        Assert.Equal(Args.Output, res.ToString());
    }

    [Fact]
    public void FloodFill_BitmapSpan_BitmapSpan()
    {
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(Args.Input);
        var bspan = bitmap.AsSpan();
        Span<uint> buffer = stackalloc uint[10];
        var actual = new BitmapSpan(bitmap.Size, buffer);
        FloodFill.FillRecursive(bspan, 1,1, actual);
        Assert.Equal(Args.Output, actual.ToString());

    }
    [Fact]
    public void FloodFill_BitmapSpan_Bitmap()
    {
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(Args.Input);
        var bspan = bitmap.AsSpan();
        var res = FloodFill.Fill(bspan, (1,1));
        Assert.Equal(Args.Output,res.ToString());

    }

    [Fact]
    public void FloodFill_Scanline()
    {
        var bitmap = (IReadOnlyBitmap)BitmapHelper.CreateFromStrings<Bitmap>(Args.Input);
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        var output = (IBitmap)new Bitmap(bitmap.Size); // Force not optimized route
#pragma warning restore CA1859
        FloodFill.FillScanline(bitmap, new VectorInt2(1, 1), output);
        Assert.Equal(Args.Output, output.ToString());
    }

    [Fact]
    public void FloodFill_Scanline_OptimizedFullRow()
    {
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(Args.Input);
        var output = new Bitmap(bitmap.Size);
        FloodFill.FillScanline(bitmap, new VectorInt2(1, 1), output);
        Assert.Equal(Args.Output, output.ToString());
    }
}




