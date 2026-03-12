using System.Diagnostics;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using VectorInt;

namespace SokoSolve.LargeSearchSolver;

public class LNodeStructEvaluatorReverse : ILNodeStructEvaluator
{
    public string GetComponentName() => GetType().Name;
    public string Describe() => "v0.1";
    public bool IsThreadSafe => false;
    public int StatsDuplicates { get; set; }

    public uint ReverseRootId { get; set; }

    public uint InitRoot(LSolverState state)
    {
        // See: ../../OrigonalSolver/SokoSolve.Core/Solver/ReverseEvaluator.cs

        var cratesOnSolution = state.StaticMaps.GoalMap;     // final reverse node has all crates on goals

        ref var root = ref state.Heap.Lease();
        ReverseRootId = root.NodeId;
        root.SetParent(NodeStruct.NodeId_NULL);
        root.SetTypeReverse();
        root.SetMapSize(cratesOnSolution.Width, cratesOnSolution.Height);
        root.SetCrateMap(cratesOnSolution);

        // MOVE MAP
        // This is more complex, as unlike the start position, we don't know the final player position
        // So the reverse move map may not be CONTIGIOUS and have more than one zone
        // Or rather reverse move map is all possible start conditions
        //      - All Floor positions that are not goals
        Debug.Assert(state.StaticMaps.FloorMap.Count > 0);
        var reverseStartPositions = state.StaticMaps.FloorMap.Clone();
        foreach(var goal in state.StaticMaps.GoalMap.TruePositions())
        {
            reverseStartPositions[goal] = false;
        }
        root.SetMoveMap(reverseStartPositions);
        root.SetPlayer(root.Width-1, root.Height-1);  // Set player to bottom-right to indicate that play is not determined at this point
        root.SetHashCode(state.HashCalculator.Calculate(ref root));
        root.SetStatus(NodeStatus.COMPLETE);

        return root.NodeId;
    }

    public void Evaluate(LSolverState state, ref NodeStruct node)
    {
        var crateMap = new Bitmap(node.Width, node.Height); // TODO: stackalloc;
        node.CopyCrateMapTo(crateMap);

        var moveMap = new Bitmap(node.Width, node.Height); // TODO: stackalloc;
        node.CopyMoveMapTo(moveMap);
        foreach (var crateBefore in crateMap.TruePositions())
        {
            foreach (var dir in VectorInt2.Directions)
            {
                //          p0 p1 p2
                // BEFORE: [M][M][C]  2xmove map next to a crate (in any direction)
                // BEFORE: [.][P][C]  +  <--
                // AFTER:  [P][C][.]
                // var p0 = crateBefore + dir + dir;
                // var p1 = crateBefore + dir;
                // var p2 = crateBefore;

                var posPlayer      = crateBefore + dir;
                var posPlayerAfter = crateBefore + dir + dir;
                var crateAfter     = crateBefore + dir;

                if (moveMap[posPlayer] && moveMap[posPlayerAfter])  // Match : Accessable crate with two free spaces to pull into
                {

                    var kid = new NodeStruct();
                    kid.SetParent(node.NodeId);
                    kid.SetMapSize(node.Width, node.Height);
                    kid.SetTypeReverse();

                    // Push
                    kid.SetPlayer(posPlayerAfter.X, posPlayerAfter.Y);
                    kid.SetPlayerPush(dir.X, dir.Y);
                    kid.SetCrateMap(crateMap);
                    kid.SetCrateMapAt(crateBefore.X, crateBefore.Y, false);
                    kid.SetCrateMapAt(crateAfter.X, crateAfter.Y, true);

                    // movemap
                    var cratesKid = crateMap.NewBitmapOfSize();
                    kid.CopyCrateMapTo(cratesKid);
                    var boundry = BitmapHelper.BitwiseOR(cratesKid, state.StaticMaps.WallMap);
                    var newMoveMap = FloodFill.Fill(boundry, posPlayerAfter);
                    kid.SetMoveMap(newMoveMap);

                    kid.SetHashCode(state.HashCalculator.Calculate(ref kid));

#if DEBUG
                    state.Debugger?.ChildBefore(state, ref kid);
#endif

                    if (state.Lookup.TryFind(ref kid, out var match))
                    {
                        ref var matchNode = ref state.Heap.GetById(match);
                        if (matchNode.Type == NodeStruct.NodeType_Forward)
                        {
                            ref var realKid = ref state.Heap.Lease();
                            realKid.SetFromNode(ref kid);
                            realKid.SetStatus(NodeStatus.CHAIN);
                            state.Heap.Commit(ref realKid);
                            state.Lookup.Add(ref realKid);
                            state.Backlog.Push( [realKid.NodeId] );
                            state.SolutionsChain.Add( (matchNode.NodeId, realKid.NodeId) );
                            state.Coordinator?.AssertSolution(state, matchNode.NodeId, realKid.NodeId);
#if DEBUG
                            state.Debugger?.ChildCommit(state, ref realKid);
#endif
                        }
                        else
                        {
                            // dup
                        }
                    }
                    else // not found
                    {
                        ref var realKid = ref state.Heap.Lease();
                        realKid.SetFromNode(ref kid);
                        realKid.SetStatus(NodeStatus.NEW_CHILD);
                        state.Heap.Commit(ref realKid);
                        state.Lookup.Add(ref realKid);
                        state.Backlog.Push( [realKid.NodeId] );

                        // solutions
                        if (cratesKid.Equals(state.StaticMaps.CrateStart))
                        {
                            realKid.SetStatus(NodeStatus.SOLUTION);
                            state.SolutionsReverse.Add(realKid.NodeId);
                            state.Coordinator?.AssertSolution(state, realKid.NodeId);
                        }
#if DEBUG
                        state.Debugger?.ChildCommit(state, ref realKid);
#endif
                    }
                }
            }
        }

        if (node.Status != NodeStatus.SOLUTION && node.Status != NodeStatus.CHAIN)
            node.SetStatus(NodeStatus.COMPLETE);
    }
}

