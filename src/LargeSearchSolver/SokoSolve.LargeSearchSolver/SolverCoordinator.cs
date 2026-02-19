using SokoSolve.Core;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib.DB;

namespace SokoSolve.LargeSearchSolver;

public record LSolverRequest(Puzzle Puzzle, SolutionDTO? Solution = null);
public class LSolverResult { }


public interface ISolverCoordinator
{
    LSolverState Init(LSolverRequest request);
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

public class SolverCoordinator : ISolverCoordinator
{
    public LSolverState Init(LSolverRequest request)
    {
        var state = new LSolverState
        {
            Request = request,

            Heap = new NodeHeap(),
            Backlog = new NodeBacklog(),
            Strategies = [ ],

            StaticMaps = new StaticAnalysisMaps(request.Puzzle),

            HashCalculator = new NodeHashCalculator()
        };

        var evalForward = new LNodeStructEvaluatorForward();

        // Init the root node
        var rootForward = evalForward.InitRoot(state);

        // Static Annalysis

        return state;
    }

    public Task<LSolverResult> Solve(LSolverState state, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
