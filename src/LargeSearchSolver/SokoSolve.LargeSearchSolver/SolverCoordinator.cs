using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib.DB;
using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, SolutionDTO? Solution = null);
public class LSolverResult
{
    public int StatusTotalNodesEvaluated { get; set; }
}


public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest request);
    Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel);
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
    void TickUpdate(LSolverState state, int totalNodes);
}

public class SolverCoordinator : ISolverCoordinator, ISolverCoordinatorCallback
{
    readonly LNodeStructEvaluatorForward evalForward = new LNodeStructEvaluatorForward();
    bool stopRequested = false;
    int solutions = 0;

    public LNodeStructEvaluatorForward Evaluator => evalForward;
    public ISolverCoodinatorPeek? Peek { get; init; }

    public bool StopOnSolution { get; set; } = true;

    public void AssertSolution(LSolverState state, uint solutionNodeId)
    {
        solutions++;
        if (StopOnSolution)
        {
            stopRequested = true;
        }
        // Console.WriteLine($"SOLUTION: {state.Heap.GetById(solutionNodeId)}");
    }

    public LSolverState Init(LSolverRequest request)
    {
        var heap = new NodeHeap();
        var state = new LSolverState
        {
            Request = request,

            Heap = heap,
            //Lookup = new LNodeLookupLinkedList(heap),
            Lookup = new LNodeLookupBlackRedTree(heap),
            Backlog = new NodeBacklog(),
            Strategies = [ ],

            StaticMaps = new StaticAnalysisMaps(request.Puzzle),

            HashCalculator = new NodeHashCalculator(),
            Coordinator = this,
        };

        // Init the root node
        var rootForward = evalForward.InitRoot(state);
        state.Backlog.Push( [rootForward] );
        state.Lookup.Add(ref state.Heap.GetById(rootForward));

        return state;
    }

    public async Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel)
    {
        int cc = 0;
        while(!stopRequested && state.Backlog.TryPop(out var nextNodeId))
        {
            ref var node = ref state.Heap.GetById(nextNodeId);

            evalForward.Evaluate(state, ref node);

            if (Peek != null && cc % Peek.PeekEvery == 0)
            {
                Peek.TickUpdate(state, cc);
            }
            cc++;
        }

        state.Result.StatusTotalNodesEvaluated = cc;

        return state.Result;
    }
}
