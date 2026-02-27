using BenchmarkDotNet.Attributes;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using VectorInt;

namespace SokoSolve.LargeSearchSolver.MicroBenchmarks;

[MemoryDiagnoser]
public class FloodFillBenchmark
{
    public class FloodFillArgs
    {
        public required string Name { get; set; }
        public required Bitmap Contraints { get; init; }
        public required VectorInt2 Start { get; init; }

        public override string ToString() => Name;
    }

    [ParamsSource(nameof(ArgsSource))]
    public FloodFillArgs Args { get; set; }

    public static IEnumerable<FloodFillArgs> ArgsSource()
    {
        Puzzle puzzle = PuzzleLibraryStatic.PQ1_P1;
        var walls = puzzle.ToMap([ puzzle.Definition.Wall, puzzle.Definition.Void ]);
        var start = puzzle.Player.Position;
        var contraints = BitmapHelper.BitwiseOR(walls, puzzle.ToMap(puzzle.Definition.AllCrates));

        yield return new FloodFillArgs
        {
            Name = "SQ1_P1",
            Contraints = contraints,
            Start = start
        };

        puzzle = PuzzleLibraryStatic.PQ1_P5;
        walls = puzzle.ToMap([ puzzle.Definition.Wall, puzzle.Definition.Void ]);
        start = puzzle.Player.Position;
        contraints = BitmapHelper.BitwiseOR(walls, puzzle.ToMap(puzzle.Definition.AllCrates));

        yield return new FloodFillArgs
        {
            Name = "SQ1_P5",
            Contraints = contraints,
            Start = start
        };
    }


    [Benchmark(Baseline = true)]
    public void Standard_Bitmap()
    {
        var actual = new Bitmap(Args.Contraints.Size);
        FloodFill.Fill(Args.Contraints, Args.Start, actual);
    }

    [Benchmark]
    public void Standard_BitmapSpan()
    {
        var c = Args.Contraints.AsSpan();
        Span<uint> buffer = stackalloc uint[c.Size.Y];
        var o = new BitmapSpan(Args.Contraints.Size, buffer);
        FloodFill.FillRecursive(c, Args.Start.X, Args.Start.Y, o);
    }
}


