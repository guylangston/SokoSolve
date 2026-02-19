using System.Diagnostics;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.LargeSearchSolver;

public class LNodeStructEvaluatorForward : ILNodeStructEvaluator
{
    readonly List<ValidPush> buffer = new List<ValidPush>(50); // thread-safety: assumes 1 instance per thread!

    public uint InitRoot(LSolverState state)
    {
        var puzzle = state.Request.Puzzle;

        var crate       = puzzle.ToMap(puzzle.Definition.AllCrates);
        var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
        var move        = FloodFill.Fill(moveBoundry, puzzle.Player.Position);

        ref var root = ref state.Heap.Lease();
        root.SetParent(uint.MaxValue);
        root.SetType(0);
        root.SetPlayerX((byte)puzzle.Player.Position.X);
        root.SetPlayerY((byte)puzzle.Player.Position.Y);
        root.SetMapSize(crate.Width, crate.Height);
        root.SetCrateMap(crate);
        root.SetMoveMap(move);
        root.SetHashCode(state.HashCalculator.Calculate(ref root));

        return root.NodeId;
    }

    struct ValidPush
    {
        public byte X;
        public byte Y;
        public sbyte dX;
        public sbyte dY;
    }

    public void Evaluate(LSolverState state, ref NodeStruct node)
    {
        // PHASE(1): Find all valid pushes
        node.SetStatus(NodeStatus.EVAL_START);

        buffer.Clear();
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
                            buffer.Add(new ValidPush()
                            {
                                X = (byte)p.X,
                                Y = (byte)p.Y,
                                dX = (sbyte)dir.X,
                                dY = (sbyte)dir.Y
                            });
                        }
                    }
                }
            }
        }
        node.SetStatus(NodeStatus.EVAL_END);

        // PHASE(2): Foreach valid push, create a child node with new crate and movemap
        var fillConstraints = new BitmapSpan(state.StaticMaps.WallMap.Size, stackalloc uint[node.Height]);
        var children = state.Heap.Lease((uint)buffer.Count);
        for(int cc=0; cc<children.Length; cc++)
        {
            var push = buffer[cc];
            ref var kid = ref children[cc];

            kid.SetStatus(NodeStatus.NEW_CHILD);
            kid.SetPlayerX((byte)(push.X + push.dX));
            kid.SetPlayerY((byte)(push.Y + push.dY));
            kid.SetPlayerPush(push.dX, push.dY);
            kid.SetMapSize(node.Width, node.Height);

            // Copy crate map, then push the crate from old to new position
            kid.SetCrateMap(ref node);
            kid.SetCrateMapAt(kid.PlayerX, kid.PlayerY, false);
            kid.SetCrateMapAt((byte)(kid.PlayerX + kid.PlayerPushX),  (byte)(kid.PlayerY + kid.PlayerPushY), true);

            // New Move map
            kid.GenerateMoveMapAndHash(state.StaticMaps.WallMap);

            // Calculate Hash
            kid.SetHashCode(state.HashCalculator.Calculate(ref kid));
        }

        node.SetStatus(NodeStatus.EVAL_ALL_CHILDREN);

        // PHASE(3): Check each new child node for (direct solution, chained solutions, duplicates)
        for(int cc=0; cc<children.Length; cc++)
        {
            ref var kid = ref children[cc];

            if(state.Heap.TryGetByHashCode(ref kid, out var matchId))
            {
                Debug.Assert(kid.NodeId != matchId);
                ref var match = ref state.Heap.GetById(matchId);
                if (match.Type == kid.Type)
                {
                    // Dup
                    kid.SetStatus(NodeStatus.DUPLICATE);
                }
                else
                {
                    throw new NotImplementedException("Chained solution?");
                }
            }
        }

        // PHASE(4): Assign valid (NON-DUPS) to tree
        int? lastId = null;
        var valid = new List<uint>();
        for(int cc=0; cc<children.Length; cc++)
        {
            ref var kid = ref children[cc];
            if (kid.Status == NodeStatus.DUPLICATE) continue;

            kid.SetParent(node.NodeId);
            if (lastId == null)
            {
                node.SetFirstChildId(children[cc].NodeId);
            }
            else
            {
                children[lastId.Value].SetSiblingNextId(kid.NodeId);
            }
            lastId = cc;

            valid.Add(kid.NodeId);
        }

        state.Backlog.Push(valid);

        node.SetStatus(NodeStatus.COMPELTE);
    }

}


