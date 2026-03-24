using VectorInt;

namespace SokoSolve.LargeSearchSolver.Tests;

public class NodeStructTests
{
    [Fact]
    public void Status()
    {
        var n = new NodeStruct();
        foreach (var en in Enum.GetValues<NodeStatus>())
        {
            n.SetStatus(en);
            Assert.Equal(en, n.Status);
        }
    }

    [Fact]
    public void Type()
    {
        var n = new NodeStruct();

        n.SetType(NodeStruct.NodeType_Reverse);
        Assert.Equal(NodeStruct.NodeType_Reverse, n.Type);

        n.SetType(NodeStruct.NodeType_Forward);
        Assert.Equal(NodeStruct.NodeType_Forward, n.Type);
    }

    [Fact]
    public void PlayerPush()
    {
        var n = new NodeStruct();
        foreach (var d in Direction.All)
        {
            VectorInt2 v = d.ToVectorInt2();
            n.SetPlayerPush((sbyte)v.X, (sbyte)v.Y);

            Assert.Equal((sbyte)v.X, n.PlayerPushX);
            Assert.Equal((sbyte)v.Y, n.PlayerPushY);
        }
    }
}


