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
        private readonly ISolverNodeFactory nodeFactory;

        public ReverseEvaluator(ISolverNodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }
        
        
        public class SolverNodeRootReverse : SolverNodeRoot
        {
            public SolverNodeRootReverse(VectorInt2 playerBefore, VectorInt2 push, Bitmap crateMap, Bitmap moveMap, INodeEvaluator evaluator, Puzzle puzzle) : base(playerBefore, push, crateMap, moveMap, evaluator, puzzle)
            {
            }
        }

        

        public SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var solution = puzzle.ToMap(puzzle.Definition.AllGoals); // START with a solution
            var walls = puzzle.ToMap(puzzle.Definition.Wall);
            // The is only one start, but MANY end soutions. Hence a single root is not a valid representation
            // We use a placeholder node (not an actualy move to hold solutions)
            var root = new SolverNodeRootReverse(
                new VectorInt2(), new VectorInt2(),
                puzzle.ToMap(puzzle.Definition.AllGoals),
                new Bitmap(puzzle.Width, puzzle.Height),
                this,
                puzzle
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
                     || puzzle[posPlayer] == puzzle.Definition.Crate)
                    &&
                    (puzzle[posPlayerAfter] == puzzle.Definition.Floor
                     || puzzle[posPlayerAfter] == puzzle.Definition.Player
                     || puzzle[posPlayer] == puzzle.Definition.Crate))
                {
                    // var crate = new Bitmap(solution);
                    // crate[crateBefore] = false;
                    // crate[crateAfter] = true;
                    // var move = FloodFill.Fill(walls.BitwiseOR(crate), posPlayerAfter);
                    // var node = nodeFactory.CreateInstance(posPlayer, posPlayerAfter - posPlayer, crate, move);
                    var node = nodeFactory.CreateFromPull(root, solution, walls, crateBefore, crateAfter, posPlayerAfter);
                    
                    if (node.MoveMap.Count > 0)
                    {
                        root.Add(node);
                        queue.Enqueue(node);
                    }
                }
            }

            return root;
        }

        public bool Evaluate(SolverState state, ISolverQueue queue, ISolverPool myPool,
            ISolverPool solutionPool, SolverNode node)
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
                if (node.CrateMap[pc] // crate to push
                    && state.StaticMaps.FloorMap[pp] && !node.CrateMap[p]
                    && !CheckDeadReverse(state, pp))
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
            SolverState state,
            ISolverPool   myPool,
            ISolverPool   solutionPool,
            SolverNode          node,
            VectorInt2          pc,
            VectorInt2          p,
            VectorInt2          pp,
            List<SolverNode>    toEnqueue, 
            ref bool            solution)
        {
            state.Statistics.TotalNodes++;


            var newKid = nodeFactory.CreateFromPull(node, node.CrateMap, state.StaticMaps.WallMap, pc, p, pp);
            

            // Cycle Check: Does this node exist already?
            var dup = myPool.FindMatch(newKid);
            if (dup != null)
            {
                // NOTE: newKid is NOT added as a ChildNode (which means less memory usage)

                // Duplicate
                newKid.Status = SolverNodeStatus.Duplicate;
                state.Statistics.Duplicates++;

                
                if (node is FatSolverNode fat) fat.AddDuplicate(dup);
                else nodeFactory.ReturnInstance(newKid); // Add to pool for later re-use?
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
                    // Add to tree / iterator
                    node.Add(newKid);  // Thread-safe: As all kids get created in this method (forward / reverse)

                    if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, node))
                    {
                        newKid.Status = SolverNodeStatus.Dead;
                    }
                    else
                    {
                        toEnqueue.Add(newKid);
                        if (newKid.IsSolutionReverse(state.StaticMaps))
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
                                throw new Exception("Invalid Solution");
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool CheckValidSolutions(SolverState state, SolverNode posibleSolution)
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

        private bool CheckDeadReverse(SolverState state, VectorInt2 ppp)
        {
            return false;
            return state.StaticMaps.DeadMap[ppp];
        }
    }
}