using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives.Analytics;

namespace SokoSolve.LargeSearchSolver;

public class LSolverState
{
    // Input
    public required LSolverRequest Request { get; init; }
    public required StaticAnalysisMaps StaticMaps { get; init; }


    // Output
    public LSolverResult Result { get; } = new();
    public List<uint> SolutionsForward { get; } = new();
    public List<uint> SolutionsReverse { get; } = new();
    public List<(uint chainForwardId, uint chainReverseId)> SolutionsChain { get; } = new();

    // Core Components
    public required INodeHeap Heap { get; set; }
    public required ILNodeLookup Lookup { get; set; }
    public required INodeBacklog Backlog { get; set; }
    public required ILNodeStructEvaluator? EvalForward { get; set; }
    public required ILNodeStructEvaluator? EvalReverse { get; set; }
    // public required IReadOnlyList<ISolverStrategy> Strategies { get; init; }

    // Components
    public required INodeHashCalculator HashCalculator { get; set; }
    public ISolverCoordinatorCallback? Coordinator { get; set; }

    // State
    public bool StopRequested { get; set; }

    // Stats
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
}

