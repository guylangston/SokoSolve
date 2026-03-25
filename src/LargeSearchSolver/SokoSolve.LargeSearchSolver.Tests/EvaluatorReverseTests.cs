using System.Text;
using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class EvaluatorReverseTests : NodeStructTestBase
{
    public EvaluatorReverseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
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
        var ctx = new NSContext(puzzle.ToMap(puzzle.Definition.AllFloors));
        var state = new LSolverState
        {
            Request = new(puzzle, new() { StopOnSolution = false }),
            NodeStructContext = ctx,
            Heap = new NodeHeap(),
            Lookup = new LNodeLookupLinkedList(heap, ctx),
            Backlog = new NodeBacklog(),
            EvalForward = null,
            EvalReverse = new LNodeStructEvaluatorReverse(),
            StaticMaps = new StaticAnalysisMaps(puzzle),
            HashCalculator = new NodeHashCalculator(ctx),
            CoordinatorCallback = null,
            Coordinator = null,
        };

        var rootRevId = state.EvalReverse.InitRoot(state);
        ref var root = ref state.Heap.GetById(rootRevId);
        Assert.Equal(NodeStruct.NodeType_Reverse, root.Type);

        var expect =
        """
        | ...... | NodeId:0 -> ParentId:(null)
        | .MCMM. | 
        | .MMMM. | REV
        | .MMCM. | COMPLETE
        | .MMMM. | dX:0, dY:0
        | .....p |

        """;
        AssertNodeReportEqual(expect, state, [ rootRevId ]);
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
        state.HashCalculator = new NodeHashCalculator(state.NodeStructContext);
        var res = coordinator.Solve(state);

        var expect =
        """
        | ...... | NodeId:0 -> ParentId:(null)
        | .MCMM. | 
        | .MMMM. | REV
        | .MMCM. | COMPLETE
        | .MMMM. | dX:0, dY:0
        | .....p |
        | ...... | NodeId:1 -> ParentId:0
        | .MMMM. | 
        | .MCMM. | REV
        | .MPCM. | NEW_CHILD
        | .MMMM. | dX:0, dY:1
        | ...... |
        | ...... | NodeId:2 -> ParentId:0
        | .MMCP. | 
        | .MMMM. | REV
        | .MMCM. | NEW_CHILD
        | .MMMM. | dX:1, dY:0
        | ...... |
        | ...... | NodeId:3 -> ParentId:0
        | .MCPM. | 
        | .MMCM. | REV
        | .MMMM. | NEW_CHILD
        | .MMMM. | dX:0, dY:-1
        | ...... |
        | ...... | NodeId:4 -> ParentId:0
        | .MCMM. | 
        | .MMMM. | REV
        | .PCMM. | NEW_CHILD
        | .MMMM. | dX:-1, dY:0
        | ...... |

        """;

        AssertNodeReportEqual(expect, state, state.Heap.EnumerateNodeIds);
    }


    [Fact]
    public void CanEvalAllNodes()
    {
        var puzzle = PuzzleLibraryStatic.Trivial01;
        var request = new LSolverRequest(puzzle, new() { StopOnSolution = false });

        var coordinator = new SolverCoordinator()
        {
            StateFactory = new SolverCoordinatorFactory()
            {
                UnitTest = true,
                Tags = new HashSet<string>([ "RevOnly" ])
            },
        };
        var state = coordinator.Init(request);
        state.EvalReverse = new LNodeStructEvaluatorReverse();
        state.EvalForward = null;
        state.HashCalculator = new NodeHashCalculator(state.NodeStructContext);
        var res = coordinator.Solve(state);

        Assert.Equal(13, res.StatusTotalNodesEvaluated);
        Assert.Single(state.SolutionsReverse);

        var expect =
        """
        | ...... | NodeId:0 -> ParentId:(null)
        | .MCMM. | 
        | .MMMM. | REV
        | .MMCM. | COMPLETE
        | .MMMM. | dX:0, dY:0
        | .....p |
        | ...... | NodeId:1 -> ParentId:0
        | .MMMM. | 
        | .MCMM. | REV
        | .MPCM. | COMPLETE
        | .MMMM. | dX:0, dY:1
        | ...... |
        | ...... | NodeId:2 -> ParentId:0
        | .MMCP. | 
        | .MMMM. | REV
        | .MMCM. | COMPLETE
        | .MMMM. | dX:1, dY:0
        | ...... |
        | ...... | NodeId:3 -> ParentId:0
        | .MCPM. | 
        | .MMCM. | REV
        | .MMMM. | COMPLETE
        | .MMMM. | dX:0, dY:-1
        | ...... |
        | ...... | NodeId:4 -> ParentId:0
        | .MCMM. | 
        | .MMMM. | REV
        | .PCMM. | COMPLETE
        | .MMMM. | dX:-1, dY:0
        | ...... |
        | ...... | NodeId:5 -> ParentId:1
        | .MMMM. | 
        | .MMMM. | REV
        | .MCCM. | COMPLETE
        | .MPMM. | dX:0, dY:1
        | ...... |
        | ...... | NodeId:6 -> ParentId:1
        | .MMMM. | 
        | .MMCP. | REV
        | .MMCM. | COMPLETE
        | .MMMM. | dX:1, dY:0
        | ...... |
        | ...... | NodeId:7 -> ParentId:1
        | .MMPM. | 
        | .MCCM. | REV
        | .MMMM. | COMPLETE
        | .MMMM. | dX:0, dY:-1
        | ...... |
        | ...... | NodeId:8 -> ParentId:1
        | .MMMM. | 
        | .MCMM. | REV
        | .PCMM. | COMPLETE
        | .MMMM. | dX:-1, dY:0
        | ...... |
        | ...... | NodeId:9 -> ParentId:2
        | .MMCM. | 
        | .MMMM. | REV
        | .PCMM. | COMPLETE
        | .MMMM. | dX:-1, dY:0
        | ...... |
        | ...... | NodeId:10 -> ParentId:3
        | .MMCP. | 
        | .MMCM. | REV
        | .MMMM. | COMPLETE
        | .MMMM. | dX:1, dY:0
        | ...... |
        | ...... | NodeId:11 -> ParentId:3
        | .MCMM. | 
        | .PCMM. | REV
        | .MMMM. | COMPLETE
        | .MMMM. | dX:-1, dY:0
        | ...... |
        | ...... | NodeId:12 -> ParentId:5
        | .MMPM. | 
        | .MMCM. | REV
        | .MCMM. | SOLUTION
        | .MMMM. | dX:0, dY:-1
        | ...... | Solution(Rev)
        | ...... | NodeId:13 -> ParentId:9
        | .MPCM. | 
        | .MCMM. | REV
        | .MMMM. | COMPLETE
        | .MMMM. | dX:0, dY:-1
        | ...... |

        """;

        AssertNodeReportEqual(expect, state, state.Heap.EnumerateNodeIds);
    }


}


