using System;
using System.Runtime.InteropServices;
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
        private Bitmap constraints;
        private VectorInt2 start;
        
        public FloodFillMicro()
        {
            var map = Puzzle.Builder.SQ1_P5();
            constraints = map.ToMap(map.Definition.Wall);
            start  = map.Player.Position;
        }

        [Benchmark(Baseline = true)]
        public void Recursive()
        {
            var filler = new BitmapFloodFillRecursive();
            var outp   = new Bitmap(constraints.Size);
            filler.Fill(constraints, start, outp);
        }
        
        [Benchmark]
        public void RecursiveOptimised1()
        {
            var outp   = new Bitmap(constraints.Size);
            FillCellAlt1(constraints, start.X, start.Y, outp);
        }
        
        [Benchmark]
        public void RecursiveOptimised2()
        {
            var outp = new Bitmap(constraints.Size);
            FillCellAlt2(constraints, start.X, start.Y, outp);
        }
        
        [Benchmark]
        public void RecursiveOptimised3()
        {
            var outp = new Bitmap(constraints.Size);
            FillCellAlt3(constraints, start.X, start.Y, outp);
        }
        
        [Benchmark]
        public void FloodSpill()
        {
            Bitmap res = new Bitmap(constraints.Size);
        
            var mark = new int[res.Width, res.Height];
            var floodParameters = new FloodParameters(startX:start.X, startY: start.Y)
            {
                NeighbourhoodType  = NeighbourhoodType.Four,
                BoundsRestriction  = new FloodBounds(constraints.Width, constraints.Height),
                Qualifier          = (x, y) => !constraints[x, y],
                NeighbourProcessor = (x, y, z) => res[x, y] = true 
            };
        
            new FloodSpiller().SpillFlood(floodParameters, mark);
        }

        [Benchmark]
        public void NativeC()
        {
            var outp = new Bitmap(constraints.Size);
            floodfill_binary((uint)constraints.Size.X, (uint)constraints.Size.Y, ref constraints.GetPointer(),  ref outp.GetPointer(), (uint)start.X, (uint)start.Y);
            
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
        
        //void floodfill_binary(unsigned sizeX, unsigned sizeY, unsigned* constraints, unsigned* target, unsigned x, unsigned y);

        [DllImport("Fill.so")]
        static extern void floodfill_binary(uint sizeX, uint sizeY, ref uint constraints, ref uint target, uint x, uint y);
        
        
        
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