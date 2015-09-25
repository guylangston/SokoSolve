using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sokoban.Core.Primitives;
using Sokoban.Core.Solver;

namespace Sokoban.Core.Analytics
{
    public class DeadMapAnalysis
    {
        public static IBitmap FindDeadMap(StaticMaps staticMaps)
        {
            var dead = new Bitmap(staticMaps.FloorMap.Size);

            // All corners are dead (provided no goals)
            foreach (var corner in staticMaps.CornerMap.TruePositions())
            {
                if (!staticMaps.GoalMap[corner]) dead[corner] = true;
            }

            // All recesses are dead (provided no goals)
            foreach (var recess in staticMaps.RecessMap)
            {
                if (!recess.Intersects(staticMaps.GoalMap))
                {
                    foreach (var cc in recess.TruePositions())
                    {
                        dead[cc] = true;
                    }
                }
            }
            

            return dead;
        }

        public static bool DynamicCheck(StaticMaps staticMaps, IStateMaps node)
        {
            var both = staticMaps.WallMap.BitwiseOR(node.CrateMap);

            // Box Rule
            foreach (var crate in node.CrateMap.TruePositions())
            {
                if (staticMaps.GoalMap[crate]) continue;
                

                if (both[crate + VectorInt2.Left] &&
                    both[crate + VectorInt2.Left + VectorInt2.Down] &&
                    both[crate + VectorInt2.Down]
                    )
                {
                    return true;
                }

                if (both[crate + VectorInt2.Left] &&
                    both[crate + VectorInt2.Left + VectorInt2.Up] &&
                    both[crate + VectorInt2.Up]
                    )
                {
                    return true;
                }

                if (both[crate + VectorInt2.Right] &&
                   both[crate + VectorInt2.Right + VectorInt2.Down] &&
                   both[crate + VectorInt2.Down]
                   )
                {
                    return true;
                }

                if (both[crate + VectorInt2.Right] &&
                    both[crate + VectorInt2.Right + VectorInt2.Up] &&
                    both[crate + VectorInt2.Up]
                    )
                {
                    return true;
                }
            }

            return false;
        }
    }
}
