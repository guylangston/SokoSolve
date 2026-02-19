using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib.DB;

namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, SolutionDTO? Solution = null);
public class LSolverResult
{
    public int TotalNodesEvaluated { get; set; }
}


public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest request);
    Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel);
}

public interface ISolverCoordinatorCallback
{
    void AssertSolution(uint solutionNodeId);
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

public class SolverCoordinator : ISolverCoordinator, ISolverCoordinatorCallback
{
    readonly LNodeStructEvaluatorForward evalForward = new LNodeStructEvaluatorForward();

    public void AssertSolution(uint solutionNodeId)
    {
        Console.WriteLine($"SOLUTION: {solutionNodeId}");
    }

    public LSolverState Init(LSolverRequest request)
    {
        var state = new LSolverState
        {
            Request = request,

            Heap = new NodeHeap(),
            Backlog = new NodeBacklog(),
            Strategies = [ ],

            StaticMaps = new StaticAnalysisMaps(request.Puzzle),

            HashCalculator = new NodeHashCalculator(),
            Coordinator = this,
        };

        // Init the root node
        var rootForward = evalForward.InitRoot(state);
        state.Backlog.Push( [rootForward] );

        return state;
    }

    public async Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel)
    {
        int cc = 0;
        while(state.Backlog.TryPop(out var nextNodeId))
        {
            ref var node = ref state.Heap.GetById(nextNodeId);

            evalForward.Evaluate(state, ref node);

            cc++;
            if (cc % 50 == 0)
            {
                var stop = 1;
            }
        }

        state.Result.TotalNodesEvaluated = cc;

        return state.Result;
    }
}
