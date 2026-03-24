using SokoSolve.Primitives;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class LNodeStructEvaluatorForwardDeadChecksTests: NodeStructTestBase
{
    public LNodeStructEvaluatorForwardDeadChecksTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }


    [Theory]
    [MemberData(nameof(IsDeadCases))]
    public void IsDead(string nodeText)
    {
        var state = SolverInit.Setup_UnitTest(PuzzleLibraryStatic.PQ1_P1, ["FwdOnly"]);
        var node = new NodeStruct();
        Assert.True(NodeStruct.TryParseDebugText(nodeText, ref node));

        Assert.True(LNodeStructEvaluatorForwardDeadChecks.IsDead(state, ref node));
    }

    public static IEnumerable<object[]> IsDeadCases()
    {
        yield return
        [
            """
            | ........... | NodeId:1 -> ParentId:0
            | ....M...... | #-609065677 stability?
            | ...MM...MM. | FWD
            | ..MMPMMMMM. | COMPLETE
            | .MMMCCM.MM. | dX:0, dY:1
            | ...MC...MM. |
            | ...MM.MMMM. |
            | ...M..M.M.. |
            | ..MMMMMM... |
            | ..MMMMM.... |
            | ........... |
            """
        ];
        yield return
        [
            """
            | ........... | NodeId:1 -> ParentId:0
            | ....M...... | #-609065677 stability?
            | ...MM...MM. | FWD
            | ..MMMMMMMM. | COMPLETE
            | .MCCPMM.MM. | dX:-1, dY:0
            | ...CM...MM. |
            | ...MM.MMMM. |
            | ...M..M.M.. |
            | ..MMMMMM... |
            | ..MMMMM.... |
            | ........... |
            """
        ];
        yield return
        [
            """
            | ........... | NodeId:1 -> ParentId:0
            | ....M...... | #-609065677 stability?
            | ...MM...MM. | FWD
            | ..MPMMMMMM. | COMPLETE
            | .MCCMMM.MM. | dX:0, dY:1
            | ...CM...MM. |
            | ...MM.MMMM. |
            | ...M..M.M.. |
            | ..MMMMMM... |
            | ..MMMMM.... |
            | ........... |
            """
        ];
    }



}


