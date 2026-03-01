using System.Diagnostics;
using SokoSolve.Core.Analytics;
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
    void Finished();
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
    T GetInstance<T>(LSolverRequest req, string? name = null);
}

public class SolverCoordinator : ISolverCoordinator, ISolverCoordinatorCallback, ISolverComponent
{
    public ISolverCoordinatorFactory StateFactory { get; init; } = new SolverCoordinatorFactory();
    public ISolverCoodinatorPeek? Peek { get; init; }
    public string GetComponentName() => nameof(SolverCoordinator);
    public string Describe() => $"{SolverVersion} {(Peek == null ? "" : "WithPeek")}";
    public static string SolverVersion => "LS-v1.1--ForwardOnly+SingleThread";

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

        var state = new LSolverState
        {
            Request = request,
            Coordinator = this,
            StaticMaps = new StaticAnalysisMaps(request.Puzzle),

            Heap = StateFactory.GetInstance<INodeHeap>(request),
            Lookup = StateFactory.GetInstance<ILNodeLookup>(request),
            Backlog = StateFactory.GetInstance<INodeBacklog>(request),
            EvalForward = StateFactory.GetInstance<ILNodeStructEvaluator>(request, "Forward"),
            EvalReverse = StateFactory.GetInstance<ILNodeStructEvaluator>(request, "Reverse"),
            HashCalculator = StateFactory.GetInstance<INodeHashCalculator>(request),
        };
        return state;
    }

    public IEnumerable<(string Name, string Desc)> DescribeComponents(LSolverState state)
    {
        yield return DescribeObj(this);
        yield return DescribeObj(state.Heap);
        yield return DescribeObj(state.Backlog);
        yield return DescribeObj(state.Lookup);
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

    public LSolverResult Solve(LSolverState state)
    {
        state.Started = DateTime.Now;

        // Init the root node
        if (state.EvalForward != null)
        {
            var rootForward = state.EvalForward.InitRoot(state);
            state.Backlog.Push( [rootForward] );
            state.Lookup.Add(ref state.Heap.GetById(rootForward));
        }

        if (state.EvalReverse != null)
        {
            var rootReverse = state.EvalReverse.InitRoot(state);
            state.Backlog.Push( [rootReverse] );
            state.Lookup.Add(ref state.Heap.GetById(rootReverse));
        }

        var cc = 0;
        var tickAt = Peek?.PeekEvery ?? 10_000;
        while(!state.StopRequested && state.Backlog.TryPop(out var nextNodeId))
        {
            ref var node = ref state.Heap.GetById(nextNodeId);

            if (node.Type == NodeStruct.NodeType_Forward)
            {
                state.EvalForward?.Evaluate(state, ref node);
            }
            else // NodeType_Reverse
            {
                state.EvalReverse?.Evaluate(state, ref node);
            }

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

    public Task<LSolverResult> SolveAsync(LSolverState state, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
