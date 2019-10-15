using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Game;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Core.Solver
{
    public interface INodeEvaluator
    {
        SolverNode Init(Puzzle puzzle, ISolverQueue queue);

        bool Evaluate(SolverCommandResult state, ISolverQueue queue, ISolverNodeLookup pool,
            ISolverNodeLookup solutionPool, SolverNode node);
    }

    public class ForwardEvaluator : INodeEvaluator
    {
        public bool IsDebugMode { get; set; }

        public SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var root = SolverHelper.CreateRoot(puzzle);
            queue.Enqueue(root);
            return root;
        }

        public bool Evaluate(SolverCommandResult state, ISolverQueue queue, ISolverNodeLookup pool,
            ISolverNodeLookup solutionPool, SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();

            node.Status = SolverNodeStatus.Evaluting;
            var toEnqueue = new List<SolverNode>();

            var solution = false;
            foreach (var move in node.MoveMap.TruePositions())
            foreach (var dir in VectorInt2.Directions)
            {
                var p = move;
                var pp = p + dir;
                var ppp = pp + dir;
                if (node.CrateMap[pp]) // crate to push
                    // into free space?
                    if (state.StaticMaps.FloorMap[ppp] && !node.CrateMap[ppp])
                        // Valid Push
                        if (!state.StaticMaps.DeadMap[ppp])
                        {
                            var newKid = new SolverNode
                            {
                                PlayerBefore = p,
                                PlayerAfter = pp,
                                CrateBefore = pp,
                                CrateAfter = ppp,
                                CrateMap = new Bitmap(node.CrateMap),
                                Evaluator = this
                            };
                            newKid.CrateMap[pp] = false;
                            newKid.CrateMap[ppp] = true;

                            var boundry = state.StaticMaps.WallMap.BitwiseOR(newKid.CrateMap);
                            newKid.MoveMap = FloodFill.Fill(boundry, pp);

                            newKid.Goals = newKid.CrateMap.BitwiseAND(state.StaticMaps.GoalMap).Count();

                            // Optimisation: PreCalc hash
                            newKid.EnsureHash();


                            // Cycle Check: Does this node exist already?
                            var dup = pool.FindMatch(newKid);
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
                                SolverNode match = null;
                                if (solutionPool != null) match = solutionPool.FindMatch(newKid);
                                if (match != null)
                                {
                                    // Add to tree / itterator
                                    node.Add(newKid);

                                    // Solution
                                    if (state.SolutionsWithReverse == null)
                                        state.SolutionsWithReverse = new List<SolutionChain>();
                                    var pair = new SolutionChain
                                    {
                                        ForwardNode = newKid,
                                        ReverseNode = match,
                                        FoundUsing = this
                                    };
                                    state.SolutionsWithReverse.Add(pair);
                                    solution = true;
                                    state.Command.Debug.Raise(this, SolverDebug.Solution, pair);

                                    foreach (var n in newKid.PathToRoot().Union(match.PathToRoot()))
                                        n.Status = SolverNodeStatus.SolutionPath;
                                    newKid.Status = SolverNodeStatus.Solution;
                                    match.Status = SolverNodeStatus.Solution;

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

                                        if (newKid.CrateMap.BitwiseAND(state.StaticMaps.GoalMap)
                                            .Equals(newKid.CrateMap))
                                        {
                                            // Solution
                                            state.Solutions.Add(newKid);
                                            state.Command.Debug.Raise(this, SolverDebug.Solution, newKid);
                                            solution = true;

                                            foreach (var n in newKid.PathToRoot())
                                                n.Status = SolverNodeStatus.SolutionPath;
                                            newKid.Status = SolverNodeStatus.Solution;
                                        }
                                    }
                                }
                            }
                        }
            }

            node.Status = node.HasChildren ? SolverNodeStatus.Evaluted : SolverNodeStatus.Dead;

            if (node.Status == SolverNodeStatus.Dead)
            {
                var p = node.Parent;
                if (p != null) p.CheckDead();
            }

            queue.Enqueue(toEnqueue);
            pool.Add(toEnqueue);


            return solution;
        }
    }
}