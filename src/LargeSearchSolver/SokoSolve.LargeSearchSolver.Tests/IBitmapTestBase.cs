using SokoSolve.Primitives;
namespace SokoSolve.LargeSearchSolver.Tests;

public abstract class IBitmapTestBase
{
    protected abstract IBitmap Create(int w, int h);

    [Fact]
    public void CanCreate8_8()
    {
        var b = Create(6,7);
        Assert.Equal(6, b.Width);
        Assert.Equal(7, b.Height);
    }


    [Fact]
    public void ThenWhenCreate0_0()
    {
        Assert.Throws<ArgumentException>(()=>Create(0,8));
        Assert.Throws<ArgumentException>(()=>Create(8,0));
        Assert.Throws<ArgumentException>(()=>Create(-8,1));
        Assert.Throws<ArgumentException>(()=>Create(1,-8));
    }

    [Fact]
    public void BoundsCheck()
    {
        var b = Create(3, 4);
        Assert.Throws<IndexOutOfRangeException>(() => b[3,4] );
        Assert.Throws<IndexOutOfRangeException>(() => b[3,4] = true);


        Assert.Throws<IndexOutOfRangeException>(() => b[-3,4] );
        Assert.Throws<IndexOutOfRangeException>(() => b[-3,4] = true);


        Assert.Throws<IndexOutOfRangeException>(() => b[3,-4] );
        Assert.Throws<IndexOutOfRangeException>(() => b[3,-4] = true);


        var c = Create(1, 4);
        Assert.Throws<IndexOutOfRangeException>(() => c[1,3] );
        Assert.Throws<IndexOutOfRangeException>(() => c[1,3] = true);
        Assert.Throws<IndexOutOfRangeException>(() => c[0,4] );
        Assert.Throws<IndexOutOfRangeException>(() => c[0,4] = true);
    }

    [Fact]
    public void CanUpdate()
    {
        var b = Create(11, 12);
        foreach(var p in b.ForEach())
        {
            Assert.False(p.Value);
        }

        foreach(var p in b.ForEachPosition())
        {
            b[p] = true;
        }

        var cc = 0;
        foreach(var p in b.ForEach())
        {
            Assert.True(p.Value, p.ToString());
            cc++;
        }
        Assert.Equal(cc, b.Count());
    }
}

public class IBitmap_Bitmap : IBitmapTestBase
{
    protected override IBitmap Create(int w, int h) => new Bitmap(w, h);
}

public class IBitmap_LinearBitmap : IBitmapTestBase
{
    protected override IBitmap Create(int w, int h) => new LinearBitmap(w, h);
}



