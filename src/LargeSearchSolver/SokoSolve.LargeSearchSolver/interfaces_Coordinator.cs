
using SokoSolve.LargeSearchSolver.Utils;

namespace SokoSolve.LargeSearchSolver;

public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest request);
    LSolverResult Solve(LSolverState state);
    Task<LSolverResult> SolveAsync(LSolverState state, CancellationToken cancel);
}

public interface ISolverCoordinatorFactory
{
    T? GetInstance<T>(LSolverRequest req, string? name = null);
}

public interface ISolverCoordinatorCallback
{
    void AssertSolution(LSolverState state, uint solutionNodeId);
    void AssertSolution(LSolverState state, uint chainForwardNodeId, uint chainReverseNodeID);
}

/// <summary>Provide status updates to a TUI</summary>
public interface ISolverCoodinatorPeek
{
    int PeekEvery { get; }

    /// <summary>Pic very `PeekEvery` then this method is executed</summary>
    /// <returns>false = stop solver</returns>
    bool TickUpdate(LSolverState state, int totalNodes, ref NodeStruct current);
    void Finished();
}

public interface INodeWatcher
{
    void Init(LSolverState state);
    void OnCommit(ref NodeStruct node);  // When an node is commited to the node heap
}

public interface ISolverCoordinatorDebugger
{
    void EvalStart(LSolverState state, ref NodeStruct node);
    void EvalComplete(LSolverState state, ref NodeStruct node);
    void ChildBefore(LSolverState state, ref NodeStruct tempNode);
    void ChildCommit(LSolverState state, ref NodeStruct node);
}

public class AttemptConstraints
{
    public int? MaxNodes { get; set; }
    public int? MaxTime { get; set; }
    public int? MaxDepth { get; set; }
    public float? MinRating { get; set; }
    public float? MaxRating { get; set; }

    // NOTE: C# defaults are not respected by CLI bindings and may be overwritten false
    public bool StopOnSolution { get; set; } = true;
    public bool StopOnSwap { get; set; } = true;

    public override string ToString()
    {
        return GeneralHelper.BuildFlags()
            .AddLabel(nameof(MaxNodes), MaxNodes)
            .AddLabel(nameof(MaxTime), MaxTime)
            .AddLabel(nameof(MaxDepth), MaxDepth)
            .AddLabel(nameof(MinRating), MinRating)
            .AddLabel(nameof(MaxRating), MaxRating)
            .AddIf(StopOnSolution, nameof(StopOnSolution))
            .AddIf(StopOnSwap, nameof(StopOnSwap))
            .ToString();
    }
}


// public class LSolverStateLocal
// {
//     public required LSolverState Global { get; init; }
//
//     public required uint LocalStateId { get; init; }
//     public required ISolverStrategy Strategy { get; init; }
//     public required Task Task { get; init; }
//     public CancellationToken Cancel { get; init; }
// }
//
// public interface ISolverStrategy
// {
//     LSolverStateLocal Init(LSolverState global);
//
//     /// <summary>Main/Worker Loop (threading handled by caller)</summary>
//     void Solve(LSolverStateLocal state);
// }

