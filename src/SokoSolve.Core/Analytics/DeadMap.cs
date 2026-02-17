using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Analytics
{
    public static class DeadMapAnalysis
    {
        public static IBitmap FindDeadMap(StaticAnalysisMaps staticMaps)
        {
            var dead = new Bitmap(staticMaps.FloorMap.Size);

            // All corners are dead (provided no goals)
            foreach (var corner in staticMaps.CornerMap.TruePositions())
                if (!staticMaps.GoalMap[corner])
                    dead[corner] = true;

            // All recesses are dead (provided no goals)
            foreach (var recess in staticMaps.RecessMaps)
                if (!recess.Intersects(staticMaps.GoalMap))
                    foreach (var cc in recess.TruePositions())
                        dead[cc] = true;

            return dead;
        }

        public static bool DynamicCheck(StaticMaps staticMaps, IStateMaps node)
        {
            var constraintsMap = new BitmapSpan(staticMaps.WallMap.Size, stackalloc uint[staticMaps.WallMap.Height]);
            constraintsMap.SetBitwiseOR(staticMaps.WallMap, node.CrateMap);

            return DynamicCheck(staticMaps, node, constraintsMap);
        }

        public static bool DynamicCheck(StaticMaps staticMaps, IStateMaps node, BitmapSpan both)
        {
            // Box Rule
            foreach (var crate in node.CrateMap.TruePositions())
            {
                if (staticMaps.GoalMap[crate]) continue;

                if (both[crate + VectorInt2.Left] &&
                    both[crate + VectorInt2.Left + VectorInt2.Down] &&
                    both[crate + VectorInt2.Down]
                )
                    return true;

                if (both[crate + VectorInt2.Left] &&
                    both[crate + VectorInt2.Left + VectorInt2.Up] &&
                    both[crate + VectorInt2.Up]
                )
                    return true;

                if (both[crate + VectorInt2.Right] &&
                    both[crate + VectorInt2.Right + VectorInt2.Down] &&
                    both[crate + VectorInt2.Down]
                )
                    return true;

                if (both[crate + VectorInt2.Right] &&
                    both[crate + VectorInt2.Right + VectorInt2.Up] &&
                    both[crate + VectorInt2.Up]
                )
                    return true;
            }

            return false;
        }
    }
}
