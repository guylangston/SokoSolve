using SokoSolve.Core.Analytics;
using SokoSolve.LargeSearchSolver;
namespace SokoSolve.LargeSearchSolver.Tests;

public class SolverCoordinatorTests : ISolverCoordinatorCallback
{
    LSolverState CreateStateDefault()
    {
        var puzzle = SokoSolve.Core.Lib.TestLibrary.Default.Puzzle;
        var state = new LSolverState
        {
            Request = new(puzzle),
            Heap = new NodeHeap(),
            Backlog = new NodeBacklog(),
            Strategies = [ ],
            StaticMaps = new StaticAnalysisMaps(puzzle),
            HashCalculator = new NodeHashCalculator(),
            Coordinator = this,
        };
        return state;
    }

    [Fact]
    public void CanInitRootForward()
    {
        var state = CreateStateDefault();

        var evalForward = new LNodeStructEvaluatorForward();
        var rootId = evalForward.InitRoot(state);
        Assert.Equal(0u, rootId);

        ref var rootStruct = ref state.Heap.GetById(rootId);
        Assert.NotEqual(0, rootStruct.HashCode);
        Assert.Equal(state.Request.Puzzle.Player.Position.X, rootStruct.PlayerX);
        Assert.Equal(state.Request.Puzzle.Player.Position.Y, rootStruct.PlayerY);

        Assert.Equal(state.Request.Puzzle.Width, rootStruct.Width);
        Assert.Equal(state.Request.Puzzle.Height, rootStruct.Height);
    }

    [Fact]
    public void CanInitRootForward_ThenEvalChildren()
    {
        var state = CreateStateDefault();
        var heap = (NodeHeap)state.Heap;
        var backlog = (NodeBacklog)state.Backlog;

        var evalForward = new LNodeStructEvaluatorForward();
        var rootId = evalForward.InitRoot(state);

        ref var rootStruct = ref state.Heap.GetById(rootId);

        evalForward.Evaluate(state, ref rootStruct);

        Assert.Equal(5u, heap.PeekNext());
        Assert.Equal(4, backlog.Count);
        // foreach(var id in backlog.Peek())
        // {
        //     ref var b = ref heap.GetById(id);
        //     Console.WriteLine(b.ToDebugString());
        // }
    }



    [Fact]
    public async Task CanSolveExcustive_LibDefault()
    {
        var puzzle = SokoSolve.Core.Lib.TestLibrary.Default;
        var request = new LSolverRequest(puzzle.Puzzle);

        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);

        var res = await coordinator.Solve(state, new CancellationToken());

        var realHeap = (NodeHeap)state.Heap;
        Assert.Equal(3058, res.StatusTotalNodesEvaluated);
        Assert.Equal(3058, realHeap.StatsCountLease);
        Assert.Equal(0, realHeap.StatsCountReturn);
        Assert.Equal(7187, coordinator.Evaluator.StatsDuplicates);

    }

    public void AssertSolution(LSolverState state, uint solutionNodeId)
    {
        throw new NotImplementedException();
    }
}

