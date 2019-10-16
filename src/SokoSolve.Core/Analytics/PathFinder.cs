using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Analytics
{
    /// <summary>
    ///     A.k.a MoveMap
    /// </summary>
    public static class PathFinder
    {
        public static Path Find(IBitmap bountry, VectorInt2 start, VectorInt2 end)
        {
            var evaluator = new Evaluator(bountry, end);

            var itt = new BreadthFirstItterator<Node>();
            var root = new Node
            {
                Position = start
            };
            var sol = itt.Evaluate(root,
                evaluator,
                new ExitConditions
                {
                    StopOnFirstSolution = true
                }
            );

            if (sol == null || sol.Count == 0) return null;
            return ToPath(sol.First().PathToRoot());
        }

        private static Path ToPath(List<Node> sol)
        {
            var p = new Path();
            p.AddRange(GeneralHelper.OffsetWalk(sol).Select(x => x.Item2.Position - x.Item1.Position));
            return p;
        }

        private class Node : TreeNodeBase
        {
            public VectorInt2 Position { get; set; }
        }

        private class Evaluator : INodeEvaluator<Node>
        {
            private readonly IBitmap bountry;
            private readonly IBitmap exists;
            private readonly VectorInt2 target;

            public Evaluator(IBitmap bountry, VectorInt2 target)
            {
                this.bountry = bountry;
                exists = new Bitmap(bountry);
                this.target = target;
            }

            public void Evalulate(Node node, IStragetgyItterator<Node> itterator)
            {
                if (node.Position == target)
                {
                    itterator.AddSolution(node);
                    return; // stop
                }

                if (bountry[node.Position]) return;

                CheckAndAddChild(itterator, node, VectorInt2.Up);
                CheckAndAddChild(itterator, node, VectorInt2.Down);
                CheckAndAddChild(itterator, node, VectorInt2.Left);
                CheckAndAddChild(itterator, node, VectorInt2.Right);
            }

            private void CheckAndAddChild(IStragetgyItterator<Node> itterator, Node node, VectorInt2 direction)
            {
                var possibleChild = node.Position + direction;
                if (!exists[possibleChild])
                {
                    itterator.Add((Node) node.Add(new Node
                    {
                        Position = possibleChild
                    }));
                    exists[possibleChild] = true;
                }
            }
        }
    }
}