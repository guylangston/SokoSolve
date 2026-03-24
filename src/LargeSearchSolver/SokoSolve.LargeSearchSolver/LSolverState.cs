using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;

namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, AttemptConstraints AttemptConstraints)
{
    public string? PuzzleIdent { get; set; }
    public string? TrackSolution { get; set; }
}

public class LSolverResult
{
    public int StatusTotalNodesEvaluated { get; set; }
    public string? Exit { get;  set; }
}

public class LSolverState
{
    // Input
    public required LSolverRequest Request { get; init; }
    public required StaticAnalysisMaps StaticMaps { get; init; }
    public required ISolverCoordinator Coordinator { get; init; }
    public required NSContext NodeStructContext { get; init; }

    // Output
    public LSolverResult Result { get; } = new();
    public List<uint> SolutionsForward { get; } = new();
    public List<uint> SolutionsReverse { get; } = new();
    public List<(uint chainForwardId, uint chainReverseId)> SolutionsChain { get; } = new();
    public uint RootForward { get; internal set; }
    public uint RootReverse { get; internal set; }

    // Core Components
    public required INodeHeap Heap { get; set; }
    public required ILNodeLookup Lookup { get; set; }
    public required INodeBacklog Backlog { get; set; }
    public required ILNodeStructEvaluator? EvalForward { get; set; }
    public required ILNodeStructEvaluator? EvalReverse { get; set; }
    public required INodeHashCalculator HashCalculator { get; set; }

    // Components
    public ISolverCoordinatorDebugger? Debugger { get; set; }
    public ISolverCoordinatorCallback? CoordinatorCallback { get; set; }
    public INodeWatcher? NodeWatcher { get; set; }

    // State
    public bool StopRequested { get; set; }

    // Stats
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
    public long MemAvailAtStart { get; set; }

    // Helpers
    public bool HasSolution
    {
        get
        {
            if (SolutionsForward.Count > 0) return true;
            if (SolutionsReverse.Count > 0) return true;
            if (SolutionsChain.Count > 0) return true;
            return false;
        }
    }

}

