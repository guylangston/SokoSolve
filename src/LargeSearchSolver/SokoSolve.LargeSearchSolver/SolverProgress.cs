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

    // willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.106 dotnet:10.0.3 Threads:12 x64 RELEASE
    // GIT-LOG1: commit ddf9b3e00f61750b597d4f8de7b4d6d4caa961d8
    // GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
    // GIT-LOG1: Date:   Wed Mar 25 11:56:39 2026 +0000
    // GIT-LOG1:     feat: show bits needed; fix: unneeded alloc
    // SolutionTracker: TrackSol=52+63/195(58%)
    // Completed:       18:48:04.3954642
    // Memory used:     63291MB
    // Total nodes:     1,283,330,000 at 18,960.5nodes/sec
    // Dead:            1,390,954,161
    // Result:          FAILED
    // ===[FOOTER]===
    // Puzzle | Rating | Time(sec) | Nodes      | Solutions | Machine | Version                                            |
    // SQ1~P5 | 76     | 67684.4   | 1283330000 | 0         | willow  | LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking |
    public const string CurrentTarget  = "SQ1~P5";


    public const string Benchmark      = "SQ1~P7"; // ~15mil nodes
    public const string BenchmarkMicro = "SQ1~P29"; // ~1mil nodes

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
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P25", TotalNodesSolution = 280_350_383 },

        new PuzzleSearchSize { PuzzleIdent = "SQ1~P5",  BestAttempt =        755_091_605} // willow, 5.4hr, 2026-03-01
    ];

    public static readonly IReadOnlyList<string> NextTargets = [ CurrentTarget];
}
