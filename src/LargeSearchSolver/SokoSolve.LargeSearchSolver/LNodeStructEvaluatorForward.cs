using System.Diagnostics;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.LargeSearchSolver;

public class LNodeStructEvaluatorForward : ILNodeStructEvaluator
{
    readonly List<NodeStruct> bufferList = new(50); // thread-safety: assumes 1 instance per thread!

    public int StatsDuplicates { get; set; }

    public uint InitRoot(LSolverState state)
    {
        var puzzle = state.Request.Puzzle;

        var crate       = puzzle.ToMap(puzzle.Definition.AllCrates);
        var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
        var move        = FloodFill.Fill(moveBoundry, puzzle.Player.Position);

        ref var root = ref state.Heap.Lease();
        root.SetParent(uint.MaxValue);
        root.SetType(0);
        root.SetPlayer((byte)puzzle.Player.Position.X, (byte)puzzle.Player.Position.Y);
        root.SetMapSize(crate.Width, crate.Height);
        root.SetCrateMap(crate);
        root.SetMoveMap(move);
        root.SetHashCode(state.HashCalculator.Calculate(ref root));

        return root.NodeId;
    }


    public void Evaluate(LSolverState state, ref NodeStruct node)
    {
        // PHASE(1): Find all valid pushes
        node.SetStatus(NodeStatus.EVAL_START);

        bufferList.Clear();
        for(byte y=0; y<node.Height; y++)
        {
            for(byte x=0; x<node.Width; x++)
            {
                if (node.GetMoveMapAt(x, y))
                {
                    foreach (var dir in VectorInt2.Directions)
                    {
                        var p   = new VectorInt2(x, y);                             // player_before
                        var pp  = p + dir;                                          // crate_before; player_after
                        var ppp = pp + dir;                                         // crate_after
                        if (node.GetCrateMapAt((byte)pp.X, (byte)pp.Y)              // crate to push
                                && state.StaticMaps.FloorMap[ppp]                   // into free space?
                                && !node.GetCrateMapAt((byte)ppp.X, (byte)ppp.Y)    // into free space?
                                && !state.StaticMaps.DeadMap[ppp])                  // valid Push location?
                        {
                            var temp = new NodeStruct();
                            temp.SetNodeId(NodeStruct.NodeId_NonPooled);
                            temp.SetMapSize(node.Width, node.Height);
                            temp.SetStatus(NodeStatus.NEW_CHILD);
                            temp.SetPlayer((byte)pp.X, (byte)pp.Y);
                            temp.SetPlayerPush((sbyte)dir.X, (sbyte)dir.Y);
                            bufferList.Add(temp);
                        }
                    }
                }
            }
        }

        if (bufferList.Count == 0)
        {
            node.SetStatus(NodeStatus.COMPLETE_LEAF);
            return;
        }
        node.SetStatus(NodeStatus.EVAL_END);

        // use spans now
        var buffer = bufferList.ToArray().AsSpan();

        // PHASE(2): Foreach valid push, create a child node with new crate and movemap
        var fillConstraints = new BitmapSpan(state.StaticMaps.WallMap.Size, stackalloc uint[node.Height]);
        for(int cc=0; cc<buffer.Length; cc++)
        {
            ref var kid = ref buffer[cc]; // still TEMP!

            // Copy crate map, then push the crate from old to new position
            kid.SetCrateMap(ref node);
            kid.SetCrateMapAt(kid.PlayerX, kid.PlayerY, false);
            kid.SetCrateMapAt((byte)(kid.PlayerX + kid.PlayerPushX),  (byte)(kid.PlayerY + kid.PlayerPushY), true);

            // New Move map
            kid.GenerateMoveMapAndHash(state.StaticMaps.WallMap);

            // Calculate Hash
            kid.SetHashCode(state.HashCalculator.Calculate(ref kid));
        }

        node.SetStatus(NodeStatus.EVAL_KIDS);

        // PHASE(3): Check each new child node for (direct solution, chained solutions, duplicates)
        for(int cc=0; cc<buffer.Length; cc++)
        {
            ref var kid = ref buffer[cc];

            // Dup, or chain?
            if(state.Lookup.TryFind(ref kid, out var matchId))
            {
                Debug.Assert(kid.NodeId != matchId);
                ref var match = ref state.Heap.GetById(matchId);
                if (match.Type == kid.Type)
                {
                    // Dup
                    kid.SetStatus(NodeStatus.DUPLICATE);
                    StatsDuplicates++;
                }
                else
                {
                    throw new NotImplementedException("Chained solution?");
                }
            }
        }

        // PHASE(4): Assign valid (NON-DUPS) to tree
        int? lastValidBufferIdx = null;
        for(int cc=0; cc<buffer.Length; cc++)
        {
            ref var tempKid = ref buffer[cc];
            if (tempKid.Status == NodeStatus.DUPLICATE) continue;

            ref var realKid = ref state.Heap.Lease();
            realKid.SetFromNode(ref tempKid);
            realKid.SetParent(node.NodeId);

            // Set Tree id,refs
            if (lastValidBufferIdx == null)
            {
                node.SetFirstChildId(realKid.NodeId);
            }
            else
            {
                buffer[lastValidBufferIdx.Value].SetSiblingNextId(realKid.NodeId);
            }
            lastValidBufferIdx = cc;

            state.Heap.Commit(ref realKid); // Allow the node to be searched for
            state.Lookup.Add(ref realKid);
            state.Backlog.Push([ realKid.NodeId ]);

            // Solution?
            // Seems late to check for solution, but for exhaustive tree searches, we want it COMMITTED
            var matchAllGoals = true;
            foreach(var p in state.StaticMaps.GoalMap.TruePositions())
            {
                if (!realKid.GetCrateMapAt((byte)p.X, (byte)p.Y))
                {
                    matchAllGoals = false;
                    break;
                }
            }
            if (matchAllGoals) // SOLULTION!
            {
                if(!state.Solutions.Contains(realKid.NodeId))
                {
                    state.Solutions.Add(realKid.NodeId);
                    state.Coordinator.AssertSolution(state, realKid.NodeId);
                }
            }

        }

        node.SetStatus(NodeStatus.COMPLETE);

    }

}


