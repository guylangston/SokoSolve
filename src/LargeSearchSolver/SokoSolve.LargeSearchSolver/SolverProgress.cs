namespace SokoSolve.LargeSearchSolver;

public record PuzzleSearchSize
{
    public required string PuzzleIdent { get; init; }
    public uint? TotalNodesSolution { get; init; }
    public uint? TotalNodesExhuasti { get; init; }
}

public record PuzzleProgress(string PuzzleIdent, bool Solution, string HostName, string HostDesc, uint TotalNodes, bool Exhaustive);

public static class KnownSolutions
{
    public static readonly IReadOnlyList<PuzzleSearchSize> TrueSize =
        [
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P3",  TotalNodesSolution = 946 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P1",  TotalNodesSolution = 3047 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P27", TotalNodesSolution = 3368 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P17", TotalNodesSolution = 64253 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P21", TotalNodesSolution = 137931 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P15", TotalNodesSolution = 260656 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P39", TotalNodesSolution = 788481 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P29", TotalNodesSolution = 913786 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P13", TotalNodesSolution = 1198991 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P41", TotalNodesSolution = 3077343 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P7",  TotalNodesSolution = 15529013 },
        new PuzzleSearchSize { PuzzleIdent = "SQ1~P43", TotalNodesSolution = 16701691 }
        ];
}
