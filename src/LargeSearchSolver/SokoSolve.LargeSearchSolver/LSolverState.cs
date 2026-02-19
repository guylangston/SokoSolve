using SokoSolve.Core.Analytics;

namespace SokoSolve.LargeSearchSolver;

public class LSolverState
{
    // Input
    public required LSolverRequest Request { get; init; }

    // Output
    public LSolverResult Result { get; } = new();

    // Working State
    public required INodeHeap Heap { get; init; }
    public required INodeBacklog Backlog { get; init; }
    public required IReadOnlyList<ISolverStrategy> Strategies { get; init; }

    // Static
    public required StaticAnalysisMaps StaticMaps { get; init; }

    // Components
    public required INodeHashCalculator HashCalculator { get; init; }
    public required ISolverCoordinatorCallback Coordinator { get; init; }
}

