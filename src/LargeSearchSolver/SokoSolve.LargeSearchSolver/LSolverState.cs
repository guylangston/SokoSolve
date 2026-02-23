using SokoSolve.Core.Analytics;
using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver;

public class LSolverState
{
    // Input
    public required LSolverRequest Request { get; init; }
    public required StaticAnalysisMaps StaticMaps { get; init; }

    // Output
    public LSolverResult Result { get; } = new();
    public List<uint> Solutions { get; } = new();

    // Core Components
    public required INodeHeap Heap { get; set; }
    public required ILNodeLookup Lookup { get; set; }
    public required INodeBacklog Backlog { get; set; }
    public required LNodeStructEvaluatorForward EvalForward { get; init; }
    // public required IReadOnlyList<ISolverStrategy> Strategies { get; init; }

    // Components
    public required INodeHashCalculator HashCalculator { get; set; }
    public required ISolverCoordinatorCallback Coordinator { get; set; }

    // State
    public bool StopRequested { get; set; }

    // Stats
    public DateTime Started { get; set; }
    public DateTime Ended { get; set; }
}

