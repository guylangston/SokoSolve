using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class SolverCoordinatorTestsNull : ISolverCoordinator, ISolverCoordinatorCallback
{
    public void AssertSolution(LSolverState state, uint solutionNodeId)
    {
        throw new NotImplementedException();
    }

    public void AssertSolution(LSolverState state, uint chainForwardNodeId, uint chainReverseNodeID)
    {
        throw new NotImplementedException();
    }

    public LSolverState Init(LSolverRequest request)
    {
        throw new NotImplementedException();
    }

    public LSolverResult Solve(LSolverState state)
    {
        throw new NotImplementedException();
    }

    public Task<LSolverResult> SolveAsync(LSolverState state, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}

public class SolverCoordinatorTests : NodeStructTestBase
{
    public SolverCoordinatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    LSolverState CreateStateDefault()
    {
        var puzzle = PuzzleLibraryStatic.PQ1_P1;
        var heap = new NodeHeap();
        var ctx = new NSContext(puzzle.ToMap(puzzle.Definition.AllFloors));
        var coord = new SolverCoordinatorTestsNull();
        var state = new LSolverState
        {
            Request = new(puzzle, new() { StopOnSolution = false }),
            NodeStructContext = ctx,
            Heap = new NodeHeap(),
            Lookup = new LNodeLookupLinkedList(heap, ctx),
            Backlog = new NodeBacklog(),
            EvalForward = new LNodeStructEvaluatorForwardStable(),
            EvalReverse = null,
            StaticMaps = new StaticAnalysisMaps(puzzle),
            HashCalculator = new NodeHashCalculator(ctx),
            Coordinator = coord,
            CoordinatorCallback = coord,
        };
        return state;
    }


    [Fact]
    public void CanInitRootForward()
    {
        var state = CreateStateDefault();

        var evalForward = new LNodeStructEvaluatorForwardStable();
        var rootId = evalForward.InitRoot(state);
        Assert.Equal(0u, rootId);

        ref var rootStruct = ref state.Heap.GetById(rootId);
        Assert.NotEqual(0, rootStruct.HashCode);
        Assert.Equal(state.Request.Puzzle.Player.Position.X, rootStruct.PlayerX);
        Assert.Equal(state.Request.Puzzle.Player.Position.Y, rootStruct.PlayerY);

        Assert.Equal(state.Request.Puzzle.Width, state.NodeStructContext.Width);
        Assert.Equal(state.Request.Puzzle.Height, state.NodeStructContext.Height);
    }

    [Fact]
    public void CanInitRootForward_ThenEvalChildren()
    {
        var state = CreateStateDefault();
        var heap = (NodeHeap)state.Heap;
        var backlog = (NodeBacklog)state.Backlog;

        var evalForward = new LNodeStructEvaluatorForwardStable();
        var rootId = evalForward.InitRoot(state);

        ref var rootStruct = ref state.Heap.GetById(rootId);

        evalForward.Evaluate(state, ref rootStruct);

        Assert.Equal(5u, heap.PeekNext());
        Assert.Equal(4, backlog.Count);
    }

    [Fact]
    public void CanSolveExcustive_LibDefault_FwdRev()
    {
        var puzzle = PuzzleLibraryStatic.PQ1_P1;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = true, MaxNodes = 10_000 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory()
            {
                UnitTest = true,
                // FwdRev by defaults
            },
            Peek = new TestPeek((state, count) => true),
        };
        var state = coordinator.Init(request);
        Assert.NotNull(state.EvalForward);
        Assert.NotNull(state.EvalReverse);
        var res = coordinator.Solve(state);
        Assert.True(state.HasSolution);
        Assert.True(state.Result.StatusTotalNodesEvaluated > 1300);
    }

    [Fact]
    public void CanSolveExcustive_LibDefault_RevOnly()
    {
        var puzzle = PuzzleLibraryStatic.PQ1_P1;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false, MaxNodes = 1_000 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory()
            {
                UnitTest = true,
                Tags = new HashSet<string>([ "RevOnly" ])
            },
        };
        var state = coordinator.Init(request);
        Assert.Null(state.EvalForward);
        Assert.NotNull(state.EvalReverse);
        var res = coordinator.Solve(state);
        Assert.True(state.HasSolution);
    }

    [Fact]
    public void CanSolveExcustive_LibDefault_FwdOnly()
    {
        var puzzle = PuzzleLibraryStatic.PQ1_P1;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false, MaxNodes = 1_000 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory()
            {
                UnitTest = true,
                Tags = new HashSet<string>([ "FwdOnly", "-DEAD", "FwdStable" ])
            },
        };
        var state = coordinator.Init(request);
        Assert.NotNull(state.EvalForward);
        Assert.Null(state.EvalReverse);
        var res = coordinator.Solve(state);
        Assert.True(state.HasSolution);

        var realHeap = (NodeHeap)state.Heap;
        Assert.Equal(3057, res.StatusTotalNodesEvaluated);
        Assert.Equal(3058, realHeap.StatsCountLease);
        Assert.Equal(0, realHeap.StatsCountReturn);
        // Assert.Equal(7187, state.EvalForward.StatsDuplicates);
        Assert.Equal([3055], state.SolutionsForward);
    }

    static Puzzle GetPuzzleById(string id)
    {
        return id switch
        {
            nameof(PuzzleLibraryStatic.Trivial01) => PuzzleLibraryStatic.Trivial01,
            nameof(PuzzleLibraryStatic.Trivial02) => PuzzleLibraryStatic.Trivial02,
            nameof(PuzzleLibraryStatic.PQ1_P1) => PuzzleLibraryStatic.PQ1_P1,
            _ => throw new KeyNotFoundException(id)
        };
    }

    [Theory]
    [InlineData("Trivial01")]
    [InlineData("Trivial02")]
    [InlineData("PQ1_P1")]
    public void CanSolve_FwdOnly_NoDeadChecks(string puzzleId)
    {
        var puzzle = GetPuzzleById(puzzleId);
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = true, MaxNodes = 1_000 });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory()
            {
                UnitTest = true,
                Tags = new HashSet<string>([ "FwdOnly", "-DEAD" ])
            },
        };
        var state = coordinator.Init(request);
        Assert.NotNull(state.EvalForward);
        Assert.Null(state.EvalReverse);
        var res = coordinator.Solve(state);
        Assert.True(state.HasSolution);

    }
}

