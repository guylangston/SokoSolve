using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class ReverseEvaluator : INodeEvaluator
    {
        public bool IsDebugMode { get; set; }

        public SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var solution = puzzle.ToMap(puzzle.Definition.AllGoals); // START with a solution
            var walls = puzzle.ToMap(puzzle.Definition.Wall);
            // The is only one start, but MANY end soutions. Hence a single root is not a valid representation
            // We use a placeholder node (not an actualy move to hold solutions)
            var root = new SingleThreadedReverseSolver.SyntheticReverseNode(
                new VectorInt2(), new VectorInt2(),
                new VectorInt2(),new VectorInt2(),
                puzzle.ToMap(puzzle.Definition.AllGoals),
                new Bitmap(puzzle.Width, puzzle.Height),
                -1,
                this
                );

            foreach (var crateBefore in solution.TruePositions())
            foreach (var dir in VectorInt2.Directions)
            {
                // ss
                var posPlayer = crateBefore + dir;
                var posPlayerAfter = posPlayer + dir;
                var crateAfter = crateBefore + dir;

                // Remember: In reverse mode goals are crates, and crate may be floor
                if ((puzzle[posPlayer] == puzzle.Definition.Floor
                     || puzzle[posPlayer] == puzzle.Definition.Player
                     || puzzle[posPlayer] == puzzle.Definition.Crate
                    )
                    &&
                    (puzzle[posPlayerAfter] == puzzle.Definition.Floor
                     || puzzle[posPlayerAfter] == puzzle.Definition.Player
                     || puzzle[posPlayer] == puzzle.Definition.Crate
                    )
                )
                {
                    var crate = new Bitmap(solution);
                    crate[crateBefore] = false;
                    crate[crateAfter] = true;

                    var node = new SolverNode(
                        posPlayer, posPlayerAfter,
                        crateBefore, crateAfter,
                        crate, FloodFill.Fill(walls.BitwiseOR(crate), posPlayerAfter),
                         -1,
                        this
                    );

                    if (node.MoveMap.Count > 0)
                    {
                        root.Add(node);
                        queue.Enqueue(node);
                    }
                }
            }

            return root;
        }

        public bool Evaluate(SolverCommandResult state, ISolverQueue queue, ISolverNodeLookup myPool,
            ISolverNodeLookup solutionPool, SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();

            node.Status = SolverNodeStatus.Evaluting;

            var solution = false;
            var toEnqueue = new List<SolverNode>();

            foreach (var move in node.MoveMap.TruePositions())
            foreach (var dir in VectorInt2.Directions)
            {
                var p = move;
                var pc = p + dir;
                var pp = p - dir;
                if (node.CrateMap[pc]) // crate to push
                    if (state.StaticMaps.FloorMap[pp] && !node.CrateMap[p])
                        if (!CheckDeadReverse(state, pp))
                        {
                            if (EvaluateValidPull(state, myPool, solutionPool, node, pc, p, pp, toEnqueue, ref solution)) return true;
                        }
            }

            node.Status = node.HasChildren ? SolverNodeStatus.Evaluted : SolverNodeStatus.Dead;
            if (node.Status == SolverNodeStatus.Dead && node.Parent != null)
            {
                node.Parent.CheckDead();
                if (node.Parent.Status == SolverNodeStatus.Dead)
                {
                    state.Statistics.TotalDead++;
                }
            }

            queue.Enqueue(toEnqueue);
            myPool.Add(toEnqueue);

            return solution;
        }

        private bool EvaluateValidPull(
            SolverCommandResult state,
            ISolverNodeLookup   myPool,
            ISolverNodeLookup   solutionPool,
            SolverNode          node,
            VectorInt2          pc,
            VectorInt2          p,
            VectorInt2          pp,
            List<SolverNode>    toEnqueue, 
            ref bool            solution)
        {
            var newCrate = new Bitmap(node.CrateMap);
            newCrate[pc] = false;
            newCrate[p]  = true;

            var newMove = FloodFill.Fill(state.StaticMaps.WallMap.BitwiseOR(newCrate), pp);

            var newKid = new SolverNode(
                p, pp,
                pc, p,
                newCrate, newMove,
                newCrate.BitwiseAND(state.StaticMaps.GoalMap).Count(),
                this
            );

            // Cycle Check: Does this node exist already?
            var dup = myPool.FindMatch(newKid);
            if (dup != null)
            {
                // NOTE: newKid is NOT added as a ChildNode (which means less memory usage)

                // Duplicate
                newKid.Status = SolverNodeStatus.Duplicate;
                state.Statistics.Duplicates++;

                if (IsDebugMode) node.AddDuplicate(dup);
            }
            else
            {
                var match = solutionPool?.FindMatch(newKid);
                if (match != null)
                {
                    // Add to tree / itterator
                    node.Add(newKid);

                    // Solution
                    state.SolutionsNodesReverse ??= new List<SolutionChain>();
                    var pair = new SolutionChain
                    {
                        ForwardNode = match,
                        ReverseNode = newKid,
                        FoundUsing  = this
                    };
                    state.SolutionsNodesReverse.Add(pair);
                    solution = true;
                    state.Command.Debug.Raise(this, SolverDebug.Solution, pair);

                    foreach (var n in newKid.PathToRoot().Union(match.PathToRoot()))
                        n.Status = SolverNodeStatus.SolutionPath;
                    newKid.Status = SolverNodeStatus.Solution;
                    match.Status  = SolverNodeStatus.Solution;
                    if (state.Command.ExitConditions.StopOnSolution) return true;
                }
                else
                {
                    // Add to tree / itterator
                    node.Add(newKid);

                    if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, node))
                    {
                        newKid.Status = SolverNodeStatus.Dead;
                    }
                    else
                    {
                        toEnqueue.Add(newKid);
                        if (newKid.CrateMap.BitwiseAND(state.StaticMaps.CrateStart)
                                  .Equals(newKid.CrateMap))
                        {
                            // Possible Solution: Did we start in a valid position
                            if (CheckValidSolutions(state, newKid))
                            {
                                state.SolutionsNodes.Add(newKid);
                                state.Command.Debug.Raise(this, SolverDebug.Solution, newKid);
                                solution = true;

                                foreach (var n in newKid.PathToRoot())
                                    n.Status = SolverNodeStatus.SolutionPath;
                                newKid.Status = SolverNodeStatus.Solution;
                            }
                            else
                            {
                                newKid.Status = SolverNodeStatus.InvalidSolution;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool CheckValidSolutions(SolverCommandResult state, SolverNode posibleSolution)
        {
            var b = state.StaticMaps.WallMap.BitwiseOR(state.StaticMaps.CrateStart);
            var f = state.Command.Puzzle.Player.Position;

            var path = posibleSolution.PathToRoot();
            path.Reverse();
            var start = path[0];
            var t = start.PlayerAfter;
            var first = PathFinder.Find(b, f, t);
            return first != null;
        }

        private bool CheckDeadReverse(SolverCommandResult state, VectorInt2 ppp)
        {
            return false;
            return state.StaticMaps.DeadMap[ppp];
        }
    }
}