namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, StaticAnalysis StaticAnalysis);
public class LSolverResult { }


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
}


public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest cmd);
    Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel);
}


public class LSolverStateLocal
{
    public required LSolverState Global { get; init; }

    public required uint LocalStateId { get; init; }
    public required ISolverStrategy Strategy { get; init; }
    public required Task Task { get; init; }
    public CancellationToken Cancel { get; init; }
}

public interface ISolverStrategy
{
    LSolverStateLocal Init(LSolverState global);

    // Main/Worker Loop (threading handled by caller)
    void Solve(LSolverStateLocal state);
}


