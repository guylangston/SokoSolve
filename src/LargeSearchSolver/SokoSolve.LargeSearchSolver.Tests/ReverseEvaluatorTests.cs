using System.Text;
using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class ReverseEvaluatorTests
{
    private readonly ITestOutputHelper testOutputHelper;

    public ReverseEvaluatorTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Regression_StaticFloorMap_Empty()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var st = new StaticAnalysisMaps(puzzle);

        var txtFloor = st.FloorMap.ToString();
        Assert.NotEqual(0, st.FloorMap.Count);

        var clone = st.FloorMap.Clone();
        Assert.Equal(st.FloorMap.Count, clone.Count);
        Assert.Equal(st.FloorMap, clone);
        Assert.Equal(txtFloor, clone.ToString());
    }

    [Fact]
    public void CanInitRoot()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var heap = new NodeHeap();
        var state = new LSolverState
        {
            Request = new(puzzle, new() { StopOnSolution = false }),
            Heap = new NodeHeap(),
            Lookup = new LNodeLookupLinkedList(heap),
            Backlog = new NodeBacklog(),
            EvalForward = null,
            EvalReverse = new LNodeStructEvaluatorReverse(),
            StaticMaps = new StaticAnalysisMaps(puzzle),
            HashCalculator = new NodeHashCalculator(),
            Coordinator = null,
        };

        // var rootFwdId = state.EvalForward.InitRoot(state);

        var rootRevId = state.EvalReverse.InitRoot(state);
        ref var root = ref state.Heap.GetById(rootRevId);
        Assert.Equal(NodeStruct.NodeType_Reverse, root.Type);

        var str = new StringBuilder();

        Assert.Equal(1, state.Backlog.Count);
        while(state.Backlog.TryPop(out var nextId))
        {
            ref var node = ref state.Heap.GetById(nextId);
            if (node.Type == NodeStruct.NodeType_Forward) continue;

            str.AppendLine(node.ToDebugString());
        }

        testOutputHelper.WriteLine(str.ToString());
        var expect =
"""
(null)<-0<- Hash:-123427471
P•••••
•mcmm•
•mmmm•
•mmcm•
•mmmm•
••••••
-----------------------


""";
        Assert.Equal(expect, str.ToString());
    }


    [Fact]
    public void CanSolverTrivialPuzzle()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator();
        var state = coordinator.Init(request);
        state.EvalReverse = new LNodeStructEvaluatorReverse();
        state.EvalForward = null;
        var res = coordinator.Solve(state);

        var realHeap = (NodeHeap)state.Heap;
        for(uint id=0; id<realHeap.Count; id++)
        {
            ref var node = ref state.Heap.GetById(id);
            testOutputHelper.WriteLine(node.ToDebugString());
        }

        Assert.Equal(15, state.Heap.Count);
        Assert.Equal([12], state.Solutions);

    } 

}


