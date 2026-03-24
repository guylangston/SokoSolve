using BenchmarkDotNet.Attributes;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

[MemoryDiagnoser]
public class BitmapBenchmark_Read
{
    public int Width { get; set; } = 16;
    public int Height { get; set; } = 13;

    [Benchmark(Baseline = true)]
    public void Bitmap() => Bench(new Bitmap(Width, Height));


    [Benchmark]
    public void LinearBitmap() => Bench(new LinearBitmap(Width, Height));

    [Benchmark]
    public void BitmapSpan()
    {
        Span<uint> buffer = stackalloc uint[Height];
        var bitmap = new BitmapSpan((Width, Height), buffer);
        Bench(ref bitmap);
    }

    [Benchmark]
    public void LinearBitmapSpan()
    {
        Span<byte> buffer = stackalloc byte[BitArrayHelper.CalcBytes(Width * Height)];
        var bitmap = new LinearBitmapSpan(buffer, Width, Height);
        Bench(ref bitmap);
    }

    void Bench(IBitmap bmap)
    {
        var cc = 0;
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                cc += bmap[x,y] ? 1 : 0;
            }
        }
    }
    void Bench(ref LinearBitmapSpan bmap)
    {
        var cc = 0;
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                cc += bmap[x,y] ? 1 : 0;
            }
        }
    }
    void Bench(ref BitmapSpan bmap)
    {
        var cc = 0;
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                cc += bmap[x,y] ? 1 : 0;
            }
        }
    }
}
[MemoryDiagnoser]
public class BitmapBenchmark_Write
{
    public int Width { get; set; } = 16;
    public int Height { get; set; } = 13;

    [Benchmark(Baseline = true)]
    public void Bitmap() => BenchWrite(new Bitmap(Width, Height));


    [Benchmark]
    public void LinearBitmap() => BenchWrite(new LinearBitmap(Width, Height));

    [Benchmark]
    public void BitmapSpan()
    {
        Span<uint> buffer = stackalloc uint[Height];
        var bitmap = new BitmapSpan((Width, Height), buffer);
        BenchWrite(ref bitmap);
    }

    [Benchmark]
    public void LinearBitmapSpan()
    {
        Span<byte> buffer = stackalloc byte[BitArrayHelper.CalcBytes(Width * Height)];
        var bitmap = new LinearBitmapSpan(buffer, Width, Height);
        BenchWrite(ref bitmap);
    }

    void BenchWrite(ref LinearBitmapSpan bmap)
    {
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                bmap[x,y] = true;
            }
        }
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                bmap[x,y] = false;
            }
        }
    }
    void BenchWrite(ref BitmapSpan bmap)
    {
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                bmap[x,y] = true;
            }
        }
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                bmap[x,y] = false;
            }
        }
    }

    void BenchWrite(IBitmap bmap)
    {
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                bmap[x,y] = true;
            }
        }
        for(var y=0; y<bmap.Height; y++)
        {
            for(var x=0; x<bmap.Width; x++)
            {
                bmap[x,y] = false;
            }
        }
    }
}



