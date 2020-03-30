using System.IO;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Analytics
{
    public static class PushMap
    {
        public static Result Find(PuzzleState state, VectorInt2 crate, VectorInt2 player)
        {
            return Find(state.Static, state.Current, crate, player);
        }

        public static Result Find(StaticMaps staticMaps, IStateMaps state, VectorInt2 crate, VectorInt2 player)
        {
            var start = new Node
            {
                PlayerAfter = player,
                CrateTarget = crate,
                Maps = state
            };
            var evaluator = new Evaluator(new Bitmap(state.CrateMap.Size), staticMaps);

            var itt = new BreadthFirstItterator<Node>();
            itt.Evaluate(start, evaluator,
                new ExitConditions
                {
                    StopOnFirstSolution = true
                }
            );
            return new Result
            {
                CrateMap = evaluator.CrateMap,
                Root = start
            };
        }

        public class Result
        {
            public IBitmap CrateMap { get; set; }
            public Node Root { get; set; }

            public Path? FindPlayerWalkRoute(VectorInt2 pos)
            {
                if (!CrateMap[pos]) return null;

                var match = Root.FirstOrDefault(x => x.CrateTarget == pos);
                if (match == null) throw new InvalidDataException();

                var res = new Path();
                foreach (var pair in GeneralHelper.OffsetWalk(match.PathToRoot()))
                {
                    // Path from old to new positions
                    var walk = PathFinder.Find(pair.Item1.Maps.MoveMap.Invert(), pair.Item1.PlayerAfter,
                        pair.Item2.PlayerBefore);
                    res.AddRange(walk);
                    res.Add(pair.Item2.PlayerAfter - pair.Item2.PlayerBefore);
                }

                return res;
            }

            public Path? FindCrateRoute(VectorInt2 pos)
            {
                if (!CrateMap[pos]) return null;

                var match = Root.FirstOrDefault(x => x.CrateTarget == pos);
                if (match == null) throw new InvalidDataException();

                var res = new Path();
                foreach (var pair in GeneralHelper.OffsetWalk(match.PathToRoot()))
                    res.Add(pair.Item2.CrateTarget - pair.Item1.CrateTarget);
                return res;
            }

            public override string ToString()
            {
                return CrateMap.ToString();
            }
        }

        public class Node : TreeNodeBase
        {
            public VectorInt2 PlayerBefore { get; set; }
            public VectorInt2 PlayerAfter { get; set; }
            public VectorInt2 CrateTarget { get; set; }
            public IStateMaps? Maps { get; set; }
        }

        private class Evaluator : INodeEvaluator<Node>
        {
            public Evaluator(IBitmap crateMap, StaticMaps @static)
            {
                CrateMap = crateMap;
                Static = @static;
            }

            public IBitmap CrateMap { get; }

            public StaticMaps Static { get; }

            public void Evalulate(Node node, IStragetgyItterator<Node> itterator)
            {
                var free = Static.FloorMap.Subtract(node.Maps.CrateMap);
                foreach (var move in node.Maps.MoveMap.TruePositions())
                {
                    var dir = node.CrateTarget - move;
                    if (dir == VectorInt2.Up || dir == VectorInt2.Down || dir == VectorInt2.Left ||
                        dir == VectorInt2.Right) // Next to each other
                    {
                        var p = move;
                        var pp = node.CrateTarget;
                        var ppp = node.CrateTarget + dir;

                        if (free.Contains(ppp) && free[ppp])
                        {
                            var newCrate = new Bitmap(node.Maps.CrateMap);
                            newCrate[pp] = false;
                            newCrate[ppp] = true;
                            var newMove = FloodFill.Fill(new Bitmap(Static.WallMap.BitwiseOR(newCrate)), pp);

                            var newNode = new Node
                            {
                                PlayerBefore = p,
                                PlayerAfter = pp,
                                CrateTarget = ppp,

                                Maps = new StateMaps(newCrate,newMove)
                            };

                            // Does this already exist
                            if (!CrateMap[ppp])
                            {
                                // New crate position
                                node.Add(newNode);
                                itterator.Add(newNode);
                                CrateMap[ppp] = true;
                            }
                            else
                            {
                                // the crate may exist, but we have a new move map
                                var match = node.Root().FirstOrDefault(x => x.Maps.Equals(newNode.Maps));
                                if (match == null)
                                {
                                    node.Add(newNode);
                                    itterator.Add(newNode);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}