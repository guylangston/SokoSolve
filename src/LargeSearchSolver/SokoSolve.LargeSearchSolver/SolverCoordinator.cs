using System.Diagnostics;
using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;

namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, AttemptConstraints AttemptConstraints)
{
    public string? PuzzleIdent { get; set; }
}
public class LSolverResult
{
    public int StatusTotalNodesEvaluated { get; set; }
    public string? Exit { get;  set; }
}

public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest request);
    LSolverResult Solve(LSolverState state);
    Task<LSolverResult> SolveAsync(LSolverState state, CancellationToken cancel);
}

public interface ISolverCoordinatorCallback
{
    void AssertSolution(LSolverState state, uint solutionNodeId);
    void AssertSolution(LSolverState state, uint chainForwardNodeId, uint chainReverseNodeID);
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

    /// <summary>Main/Worker Loop (threading handled by caller)</summary>
    void Solve(LSolverStateLocal state);
}

/// <summary>Provide status updates to a TUI</summary>
public interface ISolverCoodinatorPeek
{
    int PeekEvery { get; }

    /// <summary>Pic very `PeekEvery` then this method is executed</summary>
    /// <returns>false = stop solver</returns>
    bool TickUpdate(LSolverState state, int totalNodes);
    void Finished();
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
    public bool StopOnSolution { get; set; } = true;
}

public interface ISolverCoordinatorFactory
{
    T? GetInstance<T>(LSolverRequest req, string? name = null);
}

public class SolverCoordinator : ISolverCoordinator, ISolverCoordinatorCallback, ISolverComponent
{
    public ISolverCoordinatorFactory StateFactory { get; init; } = new SolverCoordinatorFactory();
    public ISolverCoodinatorPeek? Peek { get; init; }
    public string GetComponentName() => nameof(SolverCoordinator);
    public string Describe() => $"{SolverVersion} {(Peek == null ? "" : "WithPeek")}";
    public static string SolverVersion => "LS-v1.2--Forward+Reverse+SingleThread";

    public void AssertSolution(LSolverState state, uint solutionNodeId)
    {
        if (state.Request.AttemptConstraints.StopOnSolution)
        {
            state.StopRequested = true;
        }
    }

    public void AssertSolution(LSolverState state, uint chainForwardId, uint chainReverseId)
    {
        if (state.Request.AttemptConstraints.StopOnSolution)
        {
            state.StopRequested = true;
        }
    }

    public LSolverState Init(LSolverRequest request)
    {
        if (request.Puzzle.Width > NodeStruct.MaxMapWidth) throw new NotSupportedException($"Puzzle is too big. Consider recompiling with a larger `NodeStruct` setup. (PuzzleWidth:{request.Puzzle.Width} > {NodeStruct.MaxMapWidth})");
        if (request.Puzzle.Height > NodeStruct.MaxMapHeight) throw new NotSupportedException($"Puzzle is too big. Consider recompiling with a larger `NodeStruct` setup. (PuzzleWidth:{request.Puzzle.Height} > {NodeStruct.MaxMapHeight})");

        if (StateFactory is SolverCoordinatorFactory scf)
        {
            scf.InitComplete();
        }
        var state = new LSolverState
        {
            Request = request,
            Coordinator = this,
            StaticMaps = new StaticAnalysisMaps(request.Puzzle),

            Heap = StateFactory.GetInstance<INodeHeap>(request) ?? throw new NullReferenceException("Heap"),
            Lookup = StateFactory.GetInstance<ILNodeLookup>(request) ?? throw new NullReferenceException("Lookup"),
            Backlog = StateFactory.GetInstance<INodeBacklog>(request) ?? throw new NullReferenceException("Backlog"),
            EvalForward = StateFactory.GetInstance<ILNodeStructEvaluator>(request, "Forward"),
            EvalReverse = StateFactory.GetInstance<ILNodeStructEvaluator>(request, "Reverse"),
            HashCalculator = StateFactory.GetInstance<INodeHashCalculator>(request) ?? throw new NullReferenceException("Hash"),
        };
        return state;
    }

    public IEnumerable<(string Name, string Desc)> DescribeComponents(LSolverState state)
    {
        yield return DescribeObj(this);
        yield return DescribeObj(state.Heap);
        yield return DescribeObj(state.Backlog);
        yield return DescribeObj(state.Lookup);
        yield return DescribeObj(state.EvalForward, "(fwd)");
        yield return DescribeObj(state.EvalReverse, "(rev)");
        yield return DescribeObj(state.HashCalculator);
        yield return DescribeObj(StateFactory);

        (string Name, string Desc) DescribeObj<T>(T obj, string qual = "")
        {
            if (obj is ISolverComponent cmp)
            {
                return (typeof(T).Name + "->" + cmp.GetComponentName() +qual, cmp.Describe());
            }
            return (typeof(T).Name, obj?.ToString() ?? "(null)");
        }
    }

    public LSolverResult Solve(LSolverState state)
    {
        state.Started = DateTime.Now;

        // Init the root nodes (fwd, rev)
        if (state.EvalForward != null)
        {
            state.RootForward = state.EvalForward.InitRoot(state);
            ref var node = ref state.Heap.GetById(state.RootForward);
            state.Backlog.Push( [state.RootForward] );
            state.Heap.Commit(ref node);
            state.Lookup.Add(ref node);
        }
        if (state.EvalReverse != null)
        {
            state.RootReverse = state.EvalReverse.InitRoot(state);
            ref var node = ref state.Heap.GetById(state.RootReverse);
            state.Backlog.Push( [state.RootReverse] );
            state.Heap.Commit(ref node);
            state.Lookup.Add(ref node);
        }

        if (state.EvalForward != null && state.EvalReverse != null)
        {
            if (!state.Request.AttemptConstraints.StopOnSolution)
            {
                throw new NotSupportedException("Does it make sense to keep looking when we are creating dups?");
            }
        }

        var cc = 0;
        var tickAt = Peek?.PeekEvery ?? 10_000;
        while(!state.StopRequested && state.Backlog.TryPop(out var nextNodeId))
        {
            ref var node = ref state.Heap.GetById(nextNodeId);

#if DEBUG
            state.Debugger?.EvalStart(state, ref node);
#endif
            if (node.Type == NodeStruct.NodeType_Forward)
            {
                state.EvalForward?.Evaluate(state, ref node);
            }
            else // NodeType_Reverse
            {
                state.EvalReverse?.Evaluate(state, ref node);
            }
#if DEBUG
            state.Debugger?.EvalComplete(state, ref node);
#endif

            if (tickAt == 1 || cc % tickAt == 0)
            {
                if (state.Request.AttemptConstraints.MaxTime != null)
                {
                    var elapsed = DateTime.Now - state.Started;
                    if (elapsed.TotalSeconds > state.Request.AttemptConstraints.MaxTime.Value)
                    {
                        state.StopRequested = true;
                        break;
                    }
                }

                if(Peek?.TickUpdate(state, cc) == false)
                {
                    state.StopRequested = true;
                    break;
                }
            }
            cc++;
        }

        state.Result.Exit = state.StopRequested
            ? "StopRequested"
            : state.Backlog.Count == 0  ? "Exhaustive" : "Unknown";

        state.Result.StatusTotalNodesEvaluated = cc;
        state.Ended = DateTime.Now;

        return state.Result;
    }

    public Task<LSolverResult> SolveAsync(LSolverState state, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
