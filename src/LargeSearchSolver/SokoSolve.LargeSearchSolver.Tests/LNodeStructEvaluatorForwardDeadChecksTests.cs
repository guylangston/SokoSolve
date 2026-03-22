using System.Text.RegularExpressions;
using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using SokoSolve.Reporting;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public class LNodeStructEvaluatorForwardDeadChecksTests: NodeStructTests
{
    public LNodeStructEvaluatorForwardDeadChecksTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
            var realText = real.ToDebugString();

            var copy = new NodeStruct();
            Assert.True(TryParse(realText, ref copy));

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
            Assert.Equal(real.Width, copy.Width);
            Assert.Equal(real.Height, copy.Height);

            var round = copy.ToDebugString();

            Assert.Equal(realText, round, ignoreLineEndingDifferences: true);
            Assert.True(real.EqualsByRef(ref copy));
        }
    }

    [Fact]
    public void IsDead()
    {
        var state = SolverInit.Setup_UnitTest(PuzzleLibraryStatic.PQ1_P1, ["FwdOnly"]);
        var nodeText =
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
        """;
        var node = new NodeStruct();
        Assert.True(TryParse(nodeText, ref node));

        Assert.True(LNodeStructEvaluatorForwardDeadChecks.IsDead(state, ref node));

    }

    // Example:
    // var nodeText =
    //     """
    //     | ........... | NodeId:1 -> ParentId:0
    //     | ....M...... | #-609065677 stability?
    //     | ...MM...MM. | FWD
    //     | ..MPMMMMMM. | COMPLETE
    //     | .MMCMCM.MM. | dX:0, dY:1
    //     | ...MC...MM. |
    //     | ...MM.MMMM. |
    //     | ...M..M.M.. |
    //     | ..MMMMMM... |
    //     | ..MMMMM.... |
    //     | ........... |
    //     """;
    // Note: P should also set MoveMap=true
    private bool TryParse(string nodeText, ref NodeStruct node)
    {
        try
        {
            var lines = nodeText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0) return false;

            uint nodeId = 0, parentId = 0;
            int hashCode = 0;
            byte type = NodeStruct.NodeType_Forward;
            NodeStatus status = NodeStatus.ALLOC;
            sbyte playerPushX = 0, playerPushY = 0;
            byte playerX = 0, playerY = 0;

            var mapLines = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("|")) continue;

                // Find the second pipe to extract the map part
                var secondPipe = trimmed.IndexOf('|', 1);
                if (secondPipe < 0) continue;

                // Extract map content between first and second pipe
                var mapPart = trimmed.Substring(1, secondPipe - 1).Trim();
                mapLines.Add(mapPart);

                // Check for metadata after second pipe
                if (secondPipe + 1 < trimmed.Length)
                {
                    var metadata = trimmed.Substring(secondPipe + 1).Trim();
                    if (string.IsNullOrEmpty(metadata)) continue;

                    // Parse NodeId and ParentId
                    if (metadata.Contains("NodeId:"))
                    {
                        var nodeIdMatch = Regex.Match(metadata, @"NodeId:(\d+)");
                        var parentIdMatch = Regex.Match(metadata, @"ParentId:(\d+)");
                        if (nodeIdMatch.Success) nodeId = uint.Parse(nodeIdMatch.Groups[1].Value);
                        if (parentIdMatch.Success) parentId = uint.Parse(parentIdMatch.Groups[1].Value);
                    }
                    // Parse HashCode
                    else if (metadata.StartsWith("#"))
                    {
                        var hashMatch = Regex.Match(metadata, @"#(-?\d+)");
                        if (hashMatch.Success) hashCode = int.Parse(hashMatch.Groups[1].Value);
                    }
                    // Parse Type
                    else if (metadata == "FWD")
                    {
                        type = NodeStruct.NodeType_Forward;
                    }
                    else if (metadata == "REV")
                    {
                        type = NodeStruct.NodeType_Reverse;
                    }
                    // Parse Status
                    else if (Enum.TryParse<NodeStatus>(metadata, out var parsedStatus))
                    {
                        status = parsedStatus;
                    }
                    // Parse PlayerPush
                    else if (metadata.Contains("dX:"))
                    {
                        var dxMatch = Regex.Match(metadata, @"dX:(-?\d+)");
                        var dyMatch = Regex.Match(metadata, @"dY:(-?\d+)");
                        if (dxMatch.Success) playerPushX = sbyte.Parse(dxMatch.Groups[1].Value);
                        if (dyMatch.Success) playerPushY = sbyte.Parse(dyMatch.Groups[1].Value);
                    }
                }
            }

            if (mapLines.Count == 0) return false;

            // Determine map dimensions
            byte mapWidth = (byte)mapLines[0].Length;
            byte mapHeight = (byte)mapLines.Count;

            // Initialize node
            node.Reset();
            node.SetNodeId(nodeId);
            node.SetParent(parentId);
            node.SetHashCode(hashCode);
            node.SetType(type);
            node.SetStatus(status);
            node.SetPlayerPush(playerPushX, playerPushY);
            node.SetMapSize(mapWidth, mapHeight);

            // Clear all map positions first
            for (byte y = 0; y < mapHeight; y++)
            {
                for (byte x = 0; x < mapWidth; x++)
                {
                    node.SetCrateMapAt(x, y, false);
                    node.SetMoveMapAt(x, y, false);
                }
            }

            // Parse the map grid
            for (byte y = 0; y < mapHeight; y++)
            {
                var mapLine = mapLines[y];
                for (byte x = 0; x < mapLine.Length; x++)
                {
                    var ch = mapLine[x];
                    switch (ch)
                    {
                        case 'M':
                            // Moveable position (no crate)
                            node.SetMoveMapAt(x, y, true);
                            break;
                        case 'C':
                            // Crate (blocks movement)
                            node.SetCrateMapAt(x, y, true);
                            node.SetMoveMapAt(x, y, false); // Ensure move is false
                            break;
                        case 'P':
                            // Player at moveable position
                            node.SetMoveMapAt(x, y, true);
                            playerX = x;
                            playerY = y;
                            break;
                        case 'p':
                            // Player at non-moveable position
                            playerX = x;
                            playerY = y;
                            node.SetMoveMapAt(x, y, false); // Ensure move is false
                            break;
                        case '.':
                            // Empty space - default, no action needed
                            break;
                    }
                }
            }

            node.SetPlayer(playerX, playerY);

            return true;
        }
        catch
        {
            return false;
        }
    }
}


