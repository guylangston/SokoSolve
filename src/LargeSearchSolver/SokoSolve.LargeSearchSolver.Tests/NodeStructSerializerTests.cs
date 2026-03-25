using SokoSolve.Primitives;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class NodeStructSerializerTests : NodeStructTestBase
{
    public NodeStructSerializerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public void NodeStruct_CanDeserialise()
    {
        var state = SolverInit.Setup_UnitTest(PuzzleLibraryStatic.PQ1_P1, ["FwdOnly"]);
        state.Coordinator.Solve(state);
        // Console.WriteLine(SolverInit.DescribeComponents(state));

        for(uint cc=2; cc<22; cc++)
        {
            ref var real = ref state.Heap.GetById(cc);
            var realText = real.ToDebugString(state.NodeStructContext, true);

            var copy = new NodeStruct();
            Assert.True(NodeStruct.TryParseDebugText(state.NodeStructContext, realText, ref copy));

            // Assert node properties match
            Assert.Equal(real.NodeId, copy.NodeId);
            Assert.Equal(real.ParentId, copy.ParentId);
            Assert.Equal(real.HashCode, copy.HashCode);
            Assert.Equal(real.Status, copy.Status);
            Assert.Equal(real.Type, copy.Type);
            Assert.Equal(real.PlayerX, copy.PlayerX);
            Assert.Equal(real.PlayerY, copy.PlayerY);
            Assert.Equal(real.PlayerPushX, copy.PlayerPushX);
            Assert.Equal(real.PlayerPushY, copy.PlayerPushY);

            var round = copy.ToDebugString(state.NodeStructContext, true);

            Assert.Equal(realText, round, ignoreLineEndingDifferences: true);
            Assert.True(real.EqualsByRef(state.NodeStructContext, ref copy));
        }
    }
}



