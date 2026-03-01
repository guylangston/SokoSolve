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
        root.SetParent(uint.MaxValue);
        root.SetTypeReverse();
        root.SetMapSize(cratesOnSolution.Width, cratesOnSolution.Height);
        root.SetCrateMap(cratesOnSolution);
        root.SetHashCode(0);
        state.Lookup.Add(ref root);

        return root.NodeId;
    }

    public void Evaluate(LSolverState state, ref NodeStruct node)
    {
        var cratesParent = new Bitmap(node.Width, node.Height);
        node.CopyCrateMapTo(cratesParent);
        foreach (var crateBefore in cratesParent.TruePositions())
        {
            foreach (var dir in VectorInt2.Directions)
            {
                //          p0 p1 p2
                // BEFORE: [.][P][G]  +  <--
                // AFTER:  [P][G][.]

                var p0 = crateBefore + dir + dir;
                var p1 = crateBefore + dir;
                var p2 = crateBefore;

                var posPlayer      = crateBefore + dir;
                var posPlayerAfter = crateBefore + dir + dir;
                var crateAfter     = crateBefore + dir;

                bool IsNotGoalAndFloor(VectorInt2 xx) => !state.StaticMaps.GoalMap[xx] && state.StaticMaps.FloorMap[xx];

                if (cratesParent.WithinBounds(p0) && IsNotGoalAndFloor(p0) && IsNotGoalAndFloor(p1))
                {
                    var kid = new NodeStruct();
                    kid.SetParent(node.NodeId);
                    kid.SetMapSize(node.Width, node.Height);
                    kid.SetTypeReverse();

                    // Push
                    kid.SetPlayer(posPlayerAfter.X, posPlayerAfter.Y);
                    kid.SetPlayerPush(dir.X, dir.Y);
                    kid.SetCrateMap(cratesParent);
                    kid.SetCrateMapAt(crateBefore.X, crateBefore.Y, false);
                    kid.SetCrateMapAt(crateAfter.X, crateAfter.Y, true);

                    // movemap
                    var cratesKid = cratesParent.NewBitmapOfSize();
                    kid.CopyCrateMapTo(cratesKid);

                    var boundry = BitmapHelper.BitwiseOR(cratesKid, state.StaticMaps.WallMap);
                    var moveMap = FloodFill.Fill(boundry, posPlayerAfter);
                    kid.SetMoveMap(moveMap);

                    kid.SetHashCode(state.HashCalculator.Calculate(ref kid));

                    if (state.Lookup.TryFind(ref kid, out var match))
                    {
                        // dup
                    }
                    else // not found
                    {
                        ref var realKid = ref state.Heap.Lease();
                        realKid.SetFromNode(ref kid);
                        state.Lookup.Add(ref realKid);
                        state.Backlog.Push([ realKid.NodeId ]);
                    }

                }
            }
        }
    }

}

