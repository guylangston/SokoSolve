using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib.DB;
using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, AttemptConstraints AttemptConstraints);
public class LSolverResult
{
    public int StatusTotalNodesEvaluated { get; set; }
}


public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest request);
    Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel);
}

public interface ISolverComponent
{
    string GetComponentName();
    string Describe();
}

public interface ISolverCoordinatorCallback
{
    void AssertSolution(LSolverState state, uint solutionNodeId);
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

public interface ISolverCoodinatorPeek
{
    int PeekEvery { get; }

    /// <returns>false = stop solver</returns>
    bool TickUpdate(LSolverState state, int totalNodes);
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

public class SolverCoordinator : ISolverCoordinator, ISolverCoordinatorCallback, ISolverComponent
{
    public ISolverCoodinatorPeek? Peek { get; init; }
    public string GetComponentName() => nameof(SolverCoordinator);
    public string Describe() => Peek == null ? "" : "WithPeek";

    public void AssertSolution(LSolverState state, uint solutionNodeId)
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

        var heap = new NodeHeap();
        var evalForward = new LNodeStructEvaluatorForward();
        var state = new LSolverState
        {
            Request = request,

            Heap = heap,
            Lookup = new LNodeLookupBlackRedTree(heap),
            Backlog = new NodeBacklog(),
            EvalForward = evalForward,

            StaticMaps = new StaticAnalysisMaps(request.Puzzle),

            HashCalculator = new NodeHashSytemHashCode(),
            Coordinator = this,
        };

        return state;
    }

    public IEnumerable<(string Name, string Desc)> DescribeComponents(LSolverState state)
    {
        yield return DescribeObj(this);
        yield return DescribeObj(state.Heap);
        yield return DescribeObj(state.Backlog);
        yield return DescribeObj(state.EvalForward);
        yield return DescribeObj(state.HashCalculator);

        (string Name, string Desc) DescribeObj<T>(T obj)
        {
            if (obj is ISolverComponent cmp)
            {
                return (cmp.GetComponentName(), cmp.Describe());
            }
            return (typeof(T).Name, obj?.ToString() ?? "(null)");
        }
    }

    public async Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel)
    {
        state.Started = DateTime.Now;

        // Init the root node
        var rootForward = state.EvalForward.InitRoot(state);
        state.Backlog.Push( [rootForward] );
        state.Lookup.Add(ref state.Heap.GetById(rootForward));

        var cc = 0;
        var tickAt = Peek?.PeekEvery ?? 10_000;
        while(!state.StopRequested && state.Backlog.TryPop(out var nextNodeId))
        {
            ref var node = ref state.Heap.GetById(nextNodeId);

            state.EvalForward.Evaluate(state, ref node);

            if (cc % tickAt == 0)
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

        state.Result.StatusTotalNodesEvaluated = cc;
        state.Ended = DateTime.Now;

        return state.Result;
    }

}
