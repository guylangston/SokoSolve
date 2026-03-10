using System.Diagnostics;
using System.Runtime.CompilerServices;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;

namespace SokoSolve.LargeSearchSolver;

public class LNodeStructEvaluatorForwardStable : ILNodeStructEvaluator, ISolverComponent
{
    readonly List<NodeStruct> bufferList = new(50); // thread-safety: assumes 1 instance per thread!

    public string GetComponentName() => GetType().Name;
    public string Describe() => "v1.3:Stable+RevChains";
    public bool IsThreadSafe => false;
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
        root.SetStatus(NodeStatus.COMPLETE);

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
        if (node.GetCrateMapAt((byte)ppx, (byte)ppy)             // crate to push
            && state.StaticMaps.FloorMap[pppx, pppy]         // into free space?
            && !node.GetCrateMapAt((byte)pppx, (byte)pppy)   // into free space?
            && !state.StaticMaps.DeadMap[pppx, pppy])        // valid Push location?
        {
            var temp = new NodeStruct();
            temp.SetNodeId(NodeStruct.NodeId_NonPooled);
            temp.SetMapSize(node.Width, node.Height);
            temp.SetStatus(NodeStatus.NEW_CHILD);
            temp.SetPlayer((byte)ppx, (byte)ppy);
            temp.SetPlayerPush(dx,dy);
            buffer.Add(temp);
        }
    }

    public void Evaluate(LSolverState state, ref NodeStruct node)
    {
        // PHASE(1): Find all valid pushes

        bufferList.Clear();
        for(byte y=0; y<node.Height; y++)
        {
            for(byte x=0; x<node.Width; x++)
            {
                if (node.GetMoveMapAt(x, y))        // IDEA: This could be optimised per byte with a lookup table
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
            if (realKid.Status == NodeStatus.CHAIN)
            {
                // get the match again
                var revNodeId = chains!.Find(x=>x.bufferIdx == cc).matchReverseNodeId;
                state.SolutionsChain.Add( (realKid.NodeId, revNodeId) );
                state.Coordinator?.AssertSolution(state, realKid.NodeId, revNodeId);
            }
            // Seems late to check for solution, but for exhaustive tree searches, we want it COMMITTED
            if (realKid.AllCratesMatch(state.StaticMaps.GoalMap))
            {
                realKid.SetStatus(NodeStatus.SOLUTION);
                // SOLUTION
                if(!state.SolutionsForward.Contains(realKid.NodeId))  // why do we need this check, surely there cannot be dups here?
                {
                    state.SolutionsForward.Add(realKid.NodeId);
                    state.Coordinator?.AssertSolution(state, realKid.NodeId);
                }
            }
        }

        if (node.Status == NodeStatus.NEW_CHILD)
            node.SetStatus(NodeStatus.COMPLETE);
    }
}

