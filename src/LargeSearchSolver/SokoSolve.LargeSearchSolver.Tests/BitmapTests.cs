using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
namespace SokoSolve.LargeSearchSolver.Tests;

public class BitmapTests
{
    [Fact]
    public void AllFeatures()
    {
        IBitmap map = new Bitmap(10,20);
        for(int cy=0; cy<map.Height; cy++)
        {
            map[9, cy] = true;

            for(int cx=0; cx<map.Width; cx++)
            {
                map[cx, cy] = true;
            }
        }

        for(int cy=0; cy<map.Height; cy++)
        {
            Assert.True(map[9, cy]);

            for(int cx=0; cx<map.Width; cx++)
            {
                Assert.True(map[cx, cy]);
            }
        }
        Assert.Equal(200, map.Count());
    }

    [Fact]
    public void FromStringsSpan_AsSpan()
    {
        var mapStr =
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(mapStr);
        var bspan = bitmap.AsSpan();
        foreach(var p in bitmap.ForEach())
        {
            Assert.Equal(bitmap[p.pos], bspan[p.pos]);
        }
    }

    [Fact]
    public void FromStringsSpan_StackAlloc()
    {
        var mapStr =
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(mapStr);

        Span<uint> buffer = stackalloc uint[10];
        var bspan = new BitmapSpan(bitmap.Size, buffer);
        bspan.Set(bitmap);

        foreach(var p in bitmap.ForEach())
        {
            Assert.Equal(bitmap[p.pos], bspan[p.pos]);
        }
    }

    [Fact]
    public void FromStrings()
    {
        var mapStr =
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(mapStr);
        Assert.Equal(mapStr, bitmap.ToString());

        Assert.Equal(30, bitmap.Count);
    }

    [Fact]
    public void FloodFill_Bitmap()
    {
        var mapStr =
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(mapStr);

        var expect =
            """
            ........
            .XXXXXX.
            .X....X.
            .XX..XX.
            .XXXXXX.
            ........

            """;
        var res = FloodFill.Fill(bitmap, (1,1));
        Assert.Equal(expect,res.ToString());

    }

    [Fact]
    public void FloodFill_BitmapSpan_BitmapSpan()
    {
        var mapStr =
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(mapStr);
        var bspan = bitmap.AsSpan();

        Span<uint> buffer = stackalloc uint[10];
        var actual = new BitmapSpan(bitmap.Size, buffer);
        var expect =
            """
            ........
            .XXXXXX.
            .X....X.
            .XX..XX.
            .XXXXXX.
            ........

            """;
        FloodFill.FillRecursive(bspan, 1,1, actual);
        Assert.Equal(expect, actual.ToString());

    }
    [Fact]
    public void FloodFill_BitmapSpan_Bitmap()
    {
        var mapStr =
            """
            XXXXXXXX
            X......X
            X.XXXX.X
            X..XX..X
            X......X
            XXXXXXXX

            """;
        var bitmap = BitmapHelper.CreateFromStrings<Bitmap>(mapStr);
        var bspan = bitmap.AsSpan();

        var expect =
            """
            ........
            .XXXXXX.
            .X....X.
            .XX..XX.
            .XXXXXX.
            ........

            """;
        var res = FloodFill.Fill(bspan, (1,1));
        Assert.Equal(expect,res.ToString());

    }
}



