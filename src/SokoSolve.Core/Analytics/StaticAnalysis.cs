using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Primitives;
using VectorInt;
using VectorInt.Collections;

namespace SokoSolve.Core.Analytics
{
    public static class StaticAnalysis
    {
        public static double CalculateRating(Puzzle puzzle)
        {
            var floors = puzzle.Definition.AllFloors.Sum(x => puzzle.Count(x));
            var crates = puzzle.Count(puzzle.Definition.Crate) + puzzle.Count(puzzle.Definition.CrateGoal);
            return floors + Math.Pow(crates, 3);
        }

        public static List<LineBitmap> FindRecesses(StaticAnalysisMaps staticMaps) =>
            staticMaps.IndividualWalls
                      .Where(x => staticMaps.CornerMap[x.Start] && staticMaps.CornerMap[x.End])
                      .ToList();

        public static List<LineBitmap> FindRunsHorx(IBitmap source)
        {
            var res = new List<LineBitmap>();

            for (var y = 0; y < source.Size.Y; y++)
            {
                var x = 0;
                LineBitmap? run = null;
                while (x < source.Size.X)
                {
                    if (source[x, y])
                    {
                        if (run == null)
                        {
                            run = new LineBitmap(source.Size);
                            run.Start = new VectorInt2(x, y);
                            res.Add(run);
                        }

                        run[x, y] = true;
                    }
                    else
                    {
                        if (run != null)
                        {
                            run.End = new VectorInt2(x - 1, y);
                            run = null;
                        }
                    }

                    x++;
                }
            }

            res.RemoveAll(x => x.Count() == 1);
            return res;
        }


        public static List<LineBitmap> FindRunsVert(IBitmap source)
        {
            var res = new List<LineBitmap>();

            for (var x = 0; x < source.Size.X; x++)
            {
                var y = 0;
                LineBitmap? run = null;
                while (y < source.Size.Y)
                {
                    if (source[x, y])
                    {
                        if (run == null)
                        {
                            run = new LineBitmap(source.Size);
                            run.Start = new VectorInt2(x, y);
                            res.Add(run);
                        }

                        run[x, y] = true;
                    }
                    else
                    {
                        if (run != null)
                        {
                            run.End = new VectorInt2(x, y - 1);
                            run = null;
                        }
                    }

                    y++;
                }
            }

            res.RemoveAll(x => x.Count() == 1);
            return res;
        }


        public static List<LineBitmap> FindWalls(StaticAnalysisMaps staticMaps)
        {
            var cornerAndSide = staticMaps.CornerMap.BitwiseOR(staticMaps.SideMap);
            var res = new List<LineBitmap>();
            res.AddRange(FindRunsHorx(cornerAndSide));
            res.AddRange(FindRunsVert(cornerAndSide));


            return res;
        }

        public static IBitmap FindSides(StaticAnalysisMaps staticMaps)
        {
            var res = new Bitmap(staticMaps.FloorMap.Size);
            foreach (var floor in staticMaps.FloorMap.TruePositions())
            {
                if (staticMaps.CornerMap[floor]) continue; // Corners cannot be doors


                // ###
                // ?.?
                if (staticMaps.WallMap[floor + VectorInt2.Up]
                    && staticMaps.WallMap[floor + VectorInt2.Up + VectorInt2.Left]
                    && staticMaps.WallMap[floor + VectorInt2.Up + VectorInt2.Right]) res[floor] = true;

                // ?.?
                // ###
                if (staticMaps.WallMap[floor + VectorInt2.Down]
                    && staticMaps.WallMap[floor + VectorInt2.Down + VectorInt2.Left]
                    && staticMaps.WallMap[floor + VectorInt2.Down + VectorInt2.Right]) res[floor] = true;

                // #?
                // #.
                // #?
                if (staticMaps.WallMap[floor + VectorInt2.Left]
                    && staticMaps.WallMap[floor + VectorInt2.Left + VectorInt2.Up]
                    && staticMaps.WallMap[floor + VectorInt2.Left + VectorInt2.Down]) res[floor] = true;

                // ?#
                // .#
                // ?#
                if (staticMaps.WallMap[floor + VectorInt2.Right]
                    && staticMaps.WallMap[floor + VectorInt2.Right + VectorInt2.Up]
                    && staticMaps.WallMap[floor + VectorInt2.Right + VectorInt2.Down]) res[floor] = true;
            }

            return res;
        }

        public  static IBitmap FindDoors(StaticAnalysisMaps staticMaps)
        {
            var res = new Bitmap(staticMaps.FloorMap.Size);
            foreach (var floor in staticMaps.FloorMap.TruePositions())
            {
                if (staticMaps.CornerMap[floor]) continue; // Corners cannot be doors


                // #.#
                if (staticMaps.WallMap[floor + VectorInt2.Left] && staticMaps.WallMap[floor + VectorInt2.Right])
                    res[floor] = true;

                // #
                // .
                // #
                if (staticMaps.WallMap[floor + VectorInt2.Up] && staticMaps.WallMap[floor + VectorInt2.Down])
                    res[floor] = true;
            }

            return res;
        }

        public static IBitmap FindCorners(StaticMaps staticMaps)
        {
            var res = new Bitmap(staticMaps.FloorMap.Size);
            foreach (var floor in staticMaps.FloorMap.TruePositions())
            {
                // ##
                // #.
                if (staticMaps.WallMap[floor + VectorInt2.Up] &&
                    staticMaps.WallMap[floor + VectorInt2.Left]
                )
                    res[floor] = true;

                // ##
                // .#
                if (staticMaps.WallMap[floor + VectorInt2.Up] &&
                    staticMaps.WallMap[floor + VectorInt2.Right]
                )
                    res[floor] = true;

                // .#
                // ##
                if (staticMaps.WallMap[floor + VectorInt2.Down] &&
                    staticMaps.WallMap[floor + VectorInt2.Right]
                )
                    res[floor] = true;

                // #.
                // ##
                if (staticMaps.WallMap[floor + VectorInt2.Down] &&
                    staticMaps.WallMap[floor + VectorInt2.Left]
                )
                    res[floor] = true;
            }

            return res;
        }

        public static Puzzle Normalise(Puzzle puzzle)
        {
            var allFloor = puzzle.ToMap(puzzle.Definition.AllFloors);
            var trueFloor = FloodFill.Fill(allFloor.Invert(), puzzle.Player.Position);
            var wall = trueFloor.Invert();

            var norm = puzzle.Clone();
            foreach (var w in wall.TruePositions()) norm[w] = puzzle.Definition.Wall;

            return norm;
        }


        public static Puzzle RemoveOuter(Puzzle puzzle)
        {
            var str = puzzle.ToStringList();
            str.RemoveAt(0);
            str.RemoveAt(str.Count - 1);
            return Puzzle.Builder.FromLines(str.Select(x => x.Substring(1, x.Length - 2)));
        }

        public static Puzzle Cleaned(Puzzle puzzle)
        {
            var t = Normalise(puzzle);
            var r = t.Clone();
            foreach (var cell in t.ForEach())
                if (cell.Value == puzzle.Definition.Wall)
                {
                    var anyFloor = false;
                    foreach (var dir in VectorInt2.Directions)
                    {
                        var po = cell.Position + dir;
                        if (!puzzle.Contains(po)) continue;

                        var o = t[po];
                        if (puzzle.Definition.AllFloors.Contains(o))
                        {
                            anyFloor = true;
                            break;
                        }
                    }

                    if (!anyFloor) r[cell.Position] = puzzle.Definition.Void;
                }

            return r;
        }

        public static Map<float> CalculateWeightings(StaticAnalysisMaps input)
        {
            const float goal = 999;
            const float cornergoal = 1.2f;
            var res = input.GoalMap.ToMap(goal, 0);

            foreach (var corner in input.CornerMap.TruePositions())
                if (res[corner] > 0)
                    res[corner] *= cornergoal;

            res = AverageOver(res, input.FloorMap, input.GoalMap);
            foreach (var dead in input.DeadMap.TruePositions()) res[dead] = -1;
            return res;
        }

        public static Map<float> AverageOver(Map<float> input, IBitmap within, IBitmap start)
        {
            var res = new Map<float>(input);
            var done = new Bitmap(start);
            var hit = false;
            do
            {
                hit = false;
                foreach (var pos in done.TruePositions())
                foreach (var dir in VectorInt2.Directions)
                {
                    var p = pos + dir;
                    if (within[p] && !done[p])
                    {
                        float cc = 0;
                        float total = 0;
                        foreach (var around in VectorInt2.Directions)
                        {
                            var x = p + around;
                            if (within[x]) cc++;
                            if (done[x]) total += res[x];
                        }

                        if (cc > 0)
                        {
                            res[p] = total / cc;
                            done[p] = true;
                            hit = true;
                            break;
                        }
                    }
                }
            } while (hit);

            return res;
        }

        public static List<VectorInt2> IndexPositions(IBitmap map)
        {
            return map.ForEach().Where(x => x.Item2).Select(x => x.Item1).ToList();
        }
    }
}