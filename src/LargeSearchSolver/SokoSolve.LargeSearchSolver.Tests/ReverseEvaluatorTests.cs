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
        Assert.Equal(NodeStruct.NodeType_Reverse, root.NodeId);

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
1<-2<- Hash:1149002609
......
.mmmm.
.mcmm.
.mPcm.
.mmmm.
......
-----------------------

1<-3<- Hash:2025369065
......
.mmcP.
.mmmm.
.mmcm.
.mmmm.
......
-----------------------

1<-4<- Hash:-2092553871
......
.mcPm.
.mmcm.
.mmmm.
.mmmm.
......
-----------------------

1<-5<- Hash:-234249991
......
.mcmm.
.mmmm.
.Pcmm.
.mmmm.
......
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
        var res = coordinator.Solve(state);

        var realHeap = (NodeHeap)state.Heap;
        Assert.Equal(15, state.Heap.Count);
        Assert.Equal([123], state.Solutions);
    } 

}


