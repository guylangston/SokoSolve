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
/*
Puzzle     Rating       Time           Nodes
------------------------------------------------------------
SQ1~P1         19  0.0929147           3,047
SQ1~P3         20  0.0132734             946
SQ1~P17        26  0.4284716          64,253
SQ1~P27        30  0.0081957           3,368
SQ1~P21        33  0.6259788         137,931
SQ1~P29        40  5.5792518         913,786
SQ1~P39        41  4.6434921         788,481
SQ1~P13        44  7.8580941       1,198,991
SQ1~P41        45 19.5854584       3,077,343
SQ1~P15        49  1.4713828         260,656
SQ1~P7         60 147.113196      15,529,013
SQ1~P43        66 218.5122942      16,701,691
 */
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P1",   TotalNodesSolution = 3047 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P3",   TotalNodesSolution = 946 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P17",  TotalNodesSolution = 64253 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P27",  TotalNodesSolution = 3368 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P21",  TotalNodesSolution = 137931 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P29",  TotalNodesSolution = 913786 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P39",  TotalNodesSolution = 788481 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P13",  TotalNodesSolution = 1198991 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P41",  TotalNodesSolution = 3077343 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P15",  TotalNodesSolution = 260656 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P7",  TotalNodesSolution = 15529013 },
    new PuzzleSearchSize { PuzzleIdent = "SQ1~P43", TotalNodesSolution = 16701691 }
        ];
}
