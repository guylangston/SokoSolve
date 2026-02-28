namespace SokoSolve.LargeSearchSolver;

public record PuzzleSearchSize
{
    public required string PuzzleIdent { get; init; }
    public uint? TotalNodesSolution { get; init; }
    public uint? TotalNodesExhaustive { get; init; }
    public uint? BestAttempt { get; init; }
}

public record PuzzleProgress(string PuzzleIdent, bool Solution, string HostName, string HostDesc, uint TotalNodes, bool Exhaustive);

public static class KnownSolutions
{
    /// <summary>SQ1~P25 Rating:74 TotalSec:8187.2270154 TotalNodes:280,350,382</summary>
    public const string BestSuccess = "SQ1~P25";
    public const string CurrentTarget = "SQ1~P5";
    public const string Benchmark = "SQ1~P7"; // ~15mil nodes
    public const string BenchmarkMicro = "SQ1~P13"; // ~15mil nodes

    public static readonly IReadOnlyList<PuzzleSearchSize> TrueSize =
    [
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P3",  TotalNodesSolution =      946 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P1",  TotalNodesSolution =     3047 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P27", TotalNodesSolution =     3368 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P17", TotalNodesSolution =    64253 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P21", TotalNodesSolution =   137931 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P15", TotalNodesSolution =   260656 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P39", TotalNodesSolution =   788481 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P29", TotalNodesSolution =   913786 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P13", TotalNodesSolution =  1198991 },
        new PuzzleSearchSize { PuzzleIdent = "SQ4~P51", TotalNodesSolution =  2710290 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P41", TotalNodesSolution =  3077343 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P7",  TotalNodesSolution = 15529013 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P43", TotalNodesSolution = 16701691 },
        new PuzzleSearchSize { PuzzleIdent = "TR~P33",  TotalNodesSolution = 36608906 },
        new PuzzleSearchSize { PuzzleIdent = "SQ4~P45", TotalNodesSolution = 44538287 },

        new PuzzleSearchSize { PuzzleIdent = "SQ1~P25", TotalNodesSolution = 280_350_383 }, // guyzen, 8188sec, 2026-02-23
    ];

    public static readonly IReadOnlyList<string> NextTargets = ["SQ1~P25", "SQ1~P5"];
}
