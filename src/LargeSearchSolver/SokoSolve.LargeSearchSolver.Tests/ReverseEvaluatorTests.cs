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
        | ...... | NodeId:0 -> ParentId:(null)
        | .MCMM. | #-123427471 (not always stable)
        | .MMMM. | REV
        | .MMCM. | LEASED
        | .MMMM. | dX:0, dY:0
        | .....p |

        """;
        Assert.Equal(expect, str.ToString());
    }

    class TestPeek : ISolverCoodinatorPeek
    {
        readonly Func<LSolverState, int, bool> funcPeek;

        public TestPeek(Func<LSolverState, int, bool> funcPeek)
        {
            this.funcPeek = funcPeek;
        }

        public int PeekEvery => 1;

        public void Finished()
        {
        }

        public bool TickUpdate(LSolverState state, int totalNodes) => funcPeek(state, totalNodes);
    }

    [Fact]
    public void CanEvalRootNodes()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator()
        {
            Peek = new TestPeek((state, nodes) =>
                    {
                        return false; // stop after 1 node eval
                    })
        };
        var state = coordinator.Init(request);
        state.EvalReverse = new LNodeStructEvaluatorReverse();
        state.EvalForward = null;
        state.HashCalculator = new NodeHashCalculator();
        var res = coordinator.Solve(state);

        var realHeap = (NodeHeap)state.Heap;
        var sb = new StringBuilder();
        for(uint id=0; id<realHeap.Count; id++)
        {
            ref var node = ref state.Heap.GetById(id);
            sb.Append(node.ToDebugString());
        }
        testOutputHelper.WriteLine(sb.ToString());

        var expected =
        """
        | ...... | NodeId:0 -> ParentId:(null)
        | .MCMM. | #-123427471 (not always stable)
        | .MMMM. | REV
        | .MMCM. | LEASED
        | .MMMM. | dX:0, dY:0
        | .....p |
        | ...... | NodeId:1 -> ParentId:0
        | .MMMM. | #1149002609 (not always stable)
        | .MCMM. | REV
        | .MPCM. | ALLOC
        | .MMMM. | dX:0, dY:1
        | ...... |
        | ...... | NodeId:2 -> ParentId:0
        | .MMCP. | #2025369065 (not always stable)
        | .MMMM. | REV
        | .MMCM. | ALLOC
        | .MMMM. | dX:1, dY:0
        | ...... |
        | ...... | NodeId:3 -> ParentId:0
        | .MCPM. | #-2092553871 (not always stable)
        | .MMCM. | REV
        | .MMMM. | ALLOC
        | .MMMM. | dX:0, dY:-1
        | ...... |
        | ...... | NodeId:4 -> ParentId:0
        | .MCMM. | #-234249991 (not always stable)
        | .MMMM. | REV
        | .PCMM. | ALLOC
        | .MMMM. | dX:-1, dY:0
        | ...... |

        """;

        Assert.Equal(expected, sb.ToString());
    }

    [Fact]
    public void CanEvalAllNodes()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator()
        {
            Peek = new TestPeek((state, nodes) =>
                    {
                        return true; // stop after 1 node eval
                    })
        };
        var state = coordinator.Init(request);
        state.EvalReverse = new LNodeStructEvaluatorReverse();
        state.EvalForward = null;
        state.HashCalculator = new NodeHashCalculator();
        var res = coordinator.Solve(state);

        Assert.Equal(15, res.StatusTotalNodesEvaluated);
        Assert.Equal(1, state.Solutions.Count);
    }
}


