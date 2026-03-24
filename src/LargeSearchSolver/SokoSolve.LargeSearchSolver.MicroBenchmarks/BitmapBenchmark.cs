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
    public void LinearBitmap() => Bench(new BitmapLinear(Width, Height));

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
        var bitmap = new BitmapLinearSpan(buffer, Width, Height);
        Bench(ref bitmap);
    }

    [Benchmark]
    public void BitArrayHelper_GetBit()
    {
        Span<byte> buffer = stackalloc byte[BitArrayHelper.CalcBytes(Width * Height)];

        var cc = 0;
        for(var y=0; y<Height; y++)
        {
            for(var x=0; x<Width; x++)
            {
                cc += BitArrayHelper.GetBit(buffer, Width, Height, x,y) ? 1 : 0;
            }
        }
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
    void Bench(ref BitmapLinearSpan bmap)
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
    public void LinearBitmap() => BenchWrite(new BitmapLinear(Width, Height));

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
        var bitmap = new BitmapLinearSpan(buffer, Width, Height);
        BenchWrite(ref bitmap);
    }

    void BenchWrite(ref BitmapLinearSpan bmap)
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



