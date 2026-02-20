using SokoSolve.Core.Analytics;
using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver;

public class LSolverState
{
    // Input
    public required LSolverRequest Request { get; init; }

    // Output
    public LSolverResult Result { get; } = new();
    public List<uint> Solutions { get; } = new();

    // Working State
    public required INodeHeap Heap { get; init; }
    public required ILNodeLookup Lookup { get; init; }
    public required INodeBacklog Backlog { get; init; }
    public required IReadOnlyList<ISolverStrategy> Strategies { get; init; }

    // Static
    public required StaticAnalysisMaps StaticMaps { get; init; }

    // Components
    public required INodeHashCalculator HashCalculator { get; init; }
    public required ISolverCoordinatorCallback Coordinator { get; init; }
}

