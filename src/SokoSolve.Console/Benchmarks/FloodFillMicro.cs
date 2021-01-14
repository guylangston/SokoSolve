using BenchmarkDotNet.Attributes;
using FloodSpill;
using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Console.Benchmarks
{
    public class FloodFillMicro
    {
        private Bitmap bitmap;
        private VectorInt2 start;
        
        public FloodFillMicro()
        {
            var map = Puzzle.Builder.SQ1_P5();
            bitmap = map.ToMap(map.Definition.Wall);
            start  = map.Player.Position;
        }

        [Benchmark(Baseline = true)]
        public void Recursive()
        {
            var filler = new BitmapFloodFillRecursive();
            var outp   = new Bitmap(bitmap.Size);
            filler.Fill(bitmap, start, outp);
        }
        
        [Benchmark]
        public void RecursiveOptimised1()
        {
            var outp   = new Bitmap(bitmap.Size);
            FillCellAlt1(bitmap, start.X, start.Y, outp);
        }
        
        [Benchmark]
        public void RecursiveOptimised2()
        {
            var outp = new Bitmap(bitmap.Size);
            FillCellAlt2(bitmap, start.X, start.Y, outp);
        }
        
        [Benchmark]
        public void RecursiveOptimised3()
        {
            var outp = new Bitmap(bitmap.Size);
            FillCellAlt3(bitmap, start.X, start.Y, outp);
        }
        
        [Benchmark]
        public void FloodSpill()
        {
            Bitmap res = new Bitmap(bitmap.Size);

            var mark = new int[res.Width, res.Height];
            var floodParameters = new FloodParameters(startX:start.X, startY: start.Y)
            {
                NeighbourhoodType  = NeighbourhoodType.Four,
                BoundsRestriction  = new FloodBounds(bitmap.Width, bitmap.Height),
                Qualifier          = (x, y) => !bitmap[x, y],
                NeighbourProcessor = (x, y, z) => res[x, y] = true 
            };

            new FloodSpiller().SpillFlood(floodParameters, mark);
        }
        
        /*
|              Method |     Mean |     Error |    StdDev | Ratio |
|-------------------- |---------:|----------:|----------:|------:|
|           Recursive | 5.636 us | 0.0144 us | 0.0127 us |  1.00 |
| RecursiveOptimised1 | 3.255 us | 0.0203 us | 0.0190 us |  0.58 |
| RecursiveOptimised2 | 2.720 us | 0.0072 us | 0.0064 us |  0.48 |
| RecursiveOptimised3 | 2.826 us | 0.0160 us | 0.0149 us |  0.50 |
|          FloodSpill | 4.824 us | 0.0059 us | 0.0050 us |  0.86 |         
         * 
         */
        
        
        private static void FillCellAlt1(IReadOnlyBitmap constraints, int x, int y, IBitmap result)
        {
            if (x < 0 || y < 0) return;
            if (x > constraints.Size.X || y > constraints.Size.Y) return;

            if (constraints[x, y]) return;
            if (result[x, y]) return;

            result[x, y] = true;

            FillCellAlt1(constraints, x, y-1, result);
            FillCellAlt1(constraints, x, y+1, result);
            FillCellAlt1(constraints, x-1, y, result);
            FillCellAlt1(constraints, x+1, y, result);
        }
        
        private static void FillCellAlt2(IReadOnlyBitmap constraints, int x, int y, Bitmap result)
        {
            if (x < 0 || y < 0) return;
            if (x > constraints.Size.X || y > constraints.Size.Y) return;

            if (constraints[x, y]) return;
            if (result[x, y]) return;

            result[x, y] = true;

            FillCellAlt2(constraints, x, y-1, result);
            FillCellAlt2(constraints, x, y+1, result);
            FillCellAlt2(constraints, x-1, y, result);
            FillCellAlt2(constraints, x+1, y, result);
        }
        
        private static void FillCellAlt3(Bitmap constraints, int x, int y, Bitmap result)
        {
            if (x < 0 || y < 0) return;
            if (x > constraints.Size.X || y > constraints.Size.Y) return;

            if (constraints[x, y]) return;
            if (result[x, y]) return;

            result[x, y] = true;

            FillCellAlt2(constraints, x, y-1, result);
            FillCellAlt2(constraints, x, y+1, result);
            FillCellAlt2(constraints, x-1, y, result);
            FillCellAlt2(constraints, x+1, y, result);
        }
    }
}