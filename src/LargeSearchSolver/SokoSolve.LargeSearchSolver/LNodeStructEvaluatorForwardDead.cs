using System.Diagnostics;
using System.Runtime.CompilerServices;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using VectorInt;

namespace SokoSolve.LargeSearchSolver;

public class LNodeStructEvaluatorForwardDeadChecks : ILNodeStructEvaluator, ISolverComponent
{
    readonly List<NodeStruct> bufferList = new(50); // thread-safety: assumes 1 instance per thread!

    public string GetComponentName() => GetType().Name;
    public string Describe() => "v1.5:Dead";
    public bool IsThreadSafe => false;
    public int StatsDuplicates { get; private set; }
    public int StatsDead { get; private set; }
    public bool SkipDead { get; init; }

    public uint InitRoot(LSolverState state)
    {
        var puzzle = state.Request.Puzzle;

        var crate       = puzzle.ToMap(puzzle.Definition.AllCrates);
        var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
        var move        = FloodFill.Fill(moveBoundry, puzzle.Player.Position);

        ref var root = ref state.Heap.Lease();
        root.SetParent(NodeStruct.NodeId_NULL);
        root.SetStatus(NodeStatus.COMPLETE);
        root.SetType(NodeStruct.NodeType_Forward);
        root.SetPlayer((byte)puzzle.Player.Position.X, (byte)puzzle.Player.Position.Y);
        root.SetCrateMap(state.NodeStructContext, crate);
        root.SetMoveMap(state.NodeStructContext, move);
        root.SetHashCode(state.HashCalculator.Calculate(ref root));

        return root.NodeId;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void EvalPush(List<NodeStruct> buffer, LSolverState state,  ref NodeStruct node, byte x, byte y, sbyte dx, sbyte dy)
    {
        // var p = new VectorInt2(x, y);             // player_before
        // var pp = p + dir;                         // crate_before; player_after
        // var ppp = pp + dir;                       // crate_after
        var ppx = x+dx; if (ppx<0) return;
        var ppy = y+dy; if (ppy<0) return;
        var pppx = ppx+dx; if (pppx<0) return;
        var pppy = ppy+dy; if (pppy<0) return;

        // TODO: DeapMap + FloorMap checks could be merged (pre calced)
        if (node.GetCrateMapAt(state.NodeStructContext, (byte)ppx, (byte)ppy)             // crate to push
            && state.StaticMaps.FloorMap[pppx, pppy]         // into free space?
            && !node.GetCrateMapAt(state.NodeStructContext, (byte)pppx, (byte)pppy)   // into free space?
            && !state.StaticMaps.DeadMap[pppx, pppy])        // valid Push location?
        {
            var temp = new NodeStruct();
            temp.SetNodeId(NodeStruct.NodeId_NonPooled);
            temp.SetStatus(NodeStatus.NEW_CHILD);
            temp.SetPlayer((byte)ppx, (byte)ppy);
            temp.SetPlayerPush(dx,dy);
            buffer.Add(temp);
        }
    }

    public void Evaluate(LSolverState state, ref NodeStruct node)
    {
        // PHASE(1): Find all valid pushes
        node.SetStatus(NodeStatus.EVAL_START);

        bufferList.Clear();
        for(byte y=0; y<state.NodeStructContext.Height; y++)
        {
            for(byte x=0; x<state.NodeStructContext.Width; x++)
            {
                if (node.GetMoveMapAt(state.NodeStructContext, x, y))        // IDEA: This could be optimised per byte with a lookup table
                {
                    EvalPush(bufferList, state, ref node, x, y, 0, -1);
                    EvalPush(bufferList, state, ref node, x, y, 0, 1);
                    EvalPush(bufferList, state, ref node, x, y, -1, 0);
                    EvalPush(bufferList, state, ref node, x, y, 1, 0);
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
        var fillConstraints = new BitmapSpan(state.StaticMaps.WallMap.Size, stackalloc uint[state.NodeStructContext.Height]);
        for(int cc=0; cc<buffer.Length; cc++)
        {
            ref var kid = ref buffer[cc]; // still TEMP!

            // Copy crate map, then push the crate from old to new position
            kid.SetCrateMap(state.NodeStructContext, ref node);
            kid.SetCrateMapAt(state.NodeStructContext, kid.PlayerX, kid.PlayerY, false);
            kid.SetCrateMapAt(state.NodeStructContext, (byte)(kid.PlayerX + kid.PlayerPushX),  (byte)(kid.PlayerY + kid.PlayerPushY), true);

            // New Move map
            kid.GenerateMoveMapAndHash(state.NodeStructContext, state.StaticMaps.WallMap);

            if ( IsDeadDynamic(state, ref kid))
            {
                kid.SetStatus(NodeStatus.DEAD);
                StatsDead++;
            }

            // Calculate Hash
            kid.SetHashCode(state.HashCalculator.Calculate(ref kid));
        }

        node.SetStatus(NodeStatus.EVAL_KIDS);

        // PHASE(3): Check each new child node for (direct solution, chained solutions, duplicates)
        List<(int bufferIdx, uint matchReverseNodeId)>? chains = null;
        for(int cc=0; cc<buffer.Length; cc++)
        {
            ref var kid = ref buffer[cc];

            // Dup, or chain?
            if(state.Lookup.TryFind(ref kid, out var matchId))
            {
                Debug.Assert(kid.NodeId != matchId);
                ref var match = ref state.Heap.GetById(matchId);
                if (match.Type == NodeStruct.NodeType_Forward)
                {
                    // Dup
                    kid.SetStatus(NodeStatus.DUPLICATE);
                    StatsDuplicates++;
                }
                else
                {
                    kid.SetStatus(NodeStatus.CHAIN);
                    // Cannot assert solution here, as `kid` is not a real node that can be acccess via Heap
                    chains ??= new();
                    chains.Add( (cc, match.NodeId) );
                }
            }
        }

        // PHASE(4): Assign valid (NON-DUPS) to tree
        int? lastValidBufferIdx = null;
        for(int cc=0; cc<buffer.Length; cc++)
        {
            ref var tempKid = ref buffer[cc];
#if DEBUG
            state.Debugger?.ChildBefore(state, ref tempKid);
#endif
            if (tempKid.Status == NodeStatus.DUPLICATE) continue;
            if (tempKid.Status == NodeStatus.DEAD) continue;

            ref var realKid = ref state.Heap.Lease();
            realKid.SetFromNode(state.NodeStructContext, ref tempKid);
            realKid.SetParent(node.NodeId);
            // track id, so we can set sibs
            buffer[cc].SetNodeId(realKid.NodeId);

            // Set Tree id,refs
            if (lastValidBufferIdx == null)
            {
                node.SetFirstChildId(realKid.NodeId);
            }
            else
            {
                var prevTemp = buffer[lastValidBufferIdx.Value];
                ref var prevReal = ref state.Heap.GetById(prevTemp.NodeId);
                prevReal.SetSiblingNextId(realKid.NodeId);
            }
            lastValidBufferIdx = cc;

            state.Heap.Commit(ref realKid); // Allow the node to be searched for
            state.Lookup.Add(ref realKid);
            state.Backlog.Push([ realKid.NodeId ]);
            state.NodeWatcher?.OnCommit(ref node);

            // Solution?
            if (realKid.Status == NodeStatus.CHAIN)
            {
                // get the match again
                var revNodeId = chains!.Find(x=>x.bufferIdx == cc).matchReverseNodeId;
                state.SolutionsChain.Add( (realKid.NodeId, revNodeId) );
                state.CoordinatorCallback?.AssertSolution(state, realKid.NodeId, revNodeId);
                if (state.Request.AttemptConstraints.StopOnSolution)
                {
                    state.StopRequested = true;
                    break;
                }
            }
            // Seems late to check for solution, but for exhaustive tree searches, we want it COMMITTED
            if (realKid.AllCratesMatch(state.NodeStructContext, state.StaticMaps.GoalMap))
            {
                // SOLUTION
                if(!state.SolutionsForward.Contains(realKid.NodeId))
                {
                    state.SolutionsForward.Add(realKid.NodeId);
                    state.CoordinatorCallback?.AssertSolution(state, realKid.NodeId);
                    if (state.Request.AttemptConstraints.StopOnSolution)
                    {
                        state.StopRequested = true;
                        break;
                    }
                }
            }
#if DEBUG
            state.Debugger?.ChildCommit(state, ref realKid);
#endif
        }
        node.SetStatus(NodeStatus.COMPLETE);
    }

    public static bool IsDeadDynamic(LSolverState state, ref NodeStruct kid)
    {
        // changes crate
        var cX = kid.PlayerX + kid.PlayerPushX;
        var cY = kid.PlayerY + kid.PlayerPushY;

        if (IsSquareDynamic(state, ref kid, cX, cY))
        {
            return true;
        }

        // TODO:
        // IsCornerCrates
        // IsCoridorBlocked
        return false;
    }

    // CC CX XC
    // CC CX CC ... etc
    // Any 2x2 square of wall and crate where at least one crate in not on a goal
    public static bool IsSquareDynamic(LSolverState state, ref NodeStruct kid, int cX, int cY)
    {
        var dir = kid.PlayerPush;
        DirectionPath[] crateSquarePaths =
            [
                new DirectionPath( [ dir, dir.RotLeft(), dir.RotLeft().RotLeft() ] ),
                new DirectionPath( [ dir, dir.RotRight(), dir.RotRight().RotRight() ]),
            ];
        var newCrate = new VectorInt2(cX, cY);
        foreach(var path in crateSquarePaths)
        {
            var match = true;
            var nonGoal = false;
            foreach(var p in path.Follow(newCrate))
            {
                if (state.StaticMaps.WallMap[p]) continue;
                if (kid.GetCrateMapAt(state.NodeStructContext, (byte)p.X, (byte)p.Y))
                {
                    if (!state.StaticMaps.GoalMap[p]) nonGoal =  true;
                    continue;

                }
                match = false;
                break;
            }
            if (match && nonGoal)
            {
                return true;
            }
        }

        return false;
    }
}
