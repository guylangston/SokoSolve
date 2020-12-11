using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;
using Path=SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public class ForwardEvaluator : NodeEvaluator
    {
        public ForwardEvaluator(ISolverNodePoolingFactory nodePoolingFactory) : base(nodePoolingFactory)
        {
        }

        public SolverNodeRoot CreateRoot(Puzzle puzzle)
        {
            var crate       = puzzle.ToMap(puzzle.Definition.AllCrates);
            var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
            var move        = FloodFill.Fill(moveBoundry, puzzle.Player.Position);
            var root = new SolverNodeRoot(
                puzzle.Player.Position, new VectorInt2(),
                crate, move,
                this,
                puzzle
            );
            
            return root;
        }

        public override SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var root = CreateRoot(puzzle);
            queue.Enqueue(root);
            return root;
        }

        public override bool Evaluate(
            SolverState state, 
            ISolverQueue queue, 
            INodeLookup pool,
            INodeLookup? solutionPool, 
            SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();
            
            
            node.Status = SolverNodeStatus.Evaluting;
            var toEnqueue = new List<SolverNode>();        // TODO: Could be reused; optimise away? Aviod Allocations?
            var toPool = new List<SolverNode>();           // TODO: Could be reused; optimise away? Aviod Allocations?

            var solution = false;
            foreach (var move in node.MoveMap.TruePositions())
            {
                foreach (var dir in VectorInt2.Directions)
                {
                    var p   = move;
                    var pp  = p + dir;
                    var ppp = pp + dir;
                    if (node.CrateMap[pp]                                        // crate to push
                        && state.StaticMaps.FloorMap[ppp] && !node.CrateMap[ppp] // into free space?
                        && !state.StaticMaps.DeadMap[ppp])                       // Valid Push
                    {
                        if (EvaluateValidPush(state, pool, solutionPool, node, pp, ppp, p, dir, toEnqueue, toPool))
                        {
                            solution = true;
                            if (state.Command.ExitConditions.StopOnSolution)
                            {
                                return true;    // Are we ok to skip code below?    
                            }
                            
                        }
                    }
                }    
            }
            

            if (node.HasChildren)
            {
                node.Status = SolverNodeStatus.Evaluted;
                state.Statistics.TotalDead += node.CheckDead();    // Children may be evaluated as dead already
            }
            else
            {
                node.Status = SolverNodeStatus.Dead;
                state.Statistics.TotalDead++;
                if (node.Parent != null)
                {
                    state.Statistics.TotalDead += node.Parent.CheckDead();    
                }
            }

            queue.Enqueue(toEnqueue);
            pool.Add(toPool);

            return solution;
        }
        


        private bool EvaluateValidPush(SolverState state,
            INodeLookup                            pool,
            INodeLookup                            reversePool,
            SolverNode                             node,
            VectorInt2                             pp,
            VectorInt2                             ppp,
            VectorInt2                             p,
            VectorInt2                             push,
            List<SolverNode>                       toEnqueue,
            List<SolverNode>                       toPool)
        {
            
            state.Statistics.TotalNodes++;
            
            var newKid = nodePoolingFactory.CreateFromPush(node, node.CrateMap, state.StaticMaps.WallMap, p, pp, ppp, push);

            if (state.Command.Inspector != null && state.Command.Inspector(newKid))
            {
                state.Command.Report?.WriteLine(newKid.ToString());
            }

            // Cycle Check: Does this node exist already?
            var dup = pool.FindMatch(newKid);
            if (SafeMode && dup == null)
            {
                dup = ConfirmDupLookup(state, pool, node, toEnqueue, newKid);  // Fix or Throw
            }
            if (dup != null)
            {
                if (object.ReferenceEquals(dup, newKid)) throw new InvalidDataException();
                if (dup.SolverNodeId == newKid.SolverNodeId) throw new InvalidDataException();
                
                // Duplicate
                newKid.Status = SolverNodeStatus.Duplicate;
                state.Statistics.Duplicates++;

                if (state.Command.DuplicateMode == DuplicateMode.AddAsChild)
                {
                    node.Add(newKid);
                    if (newKid is ISolverNodeDuplicateLink dupLink) dupLink.Duplicate = dup;
                }
                else if (state.Command.DuplicateMode == DuplicateMode.ReuseInPool)
                {
                    nodePoolingFactory.ReturnInstance(newKid); // Add to pool for later re-use?
                }
                else // DuplicateMode.Discard
                {
                }
                
            }
            else
            {
                // These two should always be the same
                node.Add(newKid); toPool.Add(newKid);
                
                // If there is a reverse solver, checks its pool for a match, hence a Forward <-> Reverse chain, hence a solution
                var match = reversePool?.FindMatch(newKid);
                if (match != null)
                {
                    // Possible Solution: It may be a complete chain; but the chain may have the player on the wrong side
                    if (NewSolutionChain(state, newKid, match))
                    {
                        return true;
                    }
                    else
                    {
                        state.Command.Debug.Raise(this, SolverDebug.FalseSolution, new SolutionChain()
                        {
                            ForwardNode = match,
                            ReverseNode = newKid
                        });
                    }
                }
                else
                {
                    if (DeadMapAnalysis.DynamicCheck(state.StaticMaps, node))
                    {
                        newKid.Status = SolverNodeStatus.Dead;
                        state.Statistics.TotalDead++;
                    }
                    else
                    {
                        toEnqueue.Add(newKid);

                        if (newKid.IsSolutionForward(state.StaticMaps))
                        {
                            var path = SolverHelper.ConvertForwardNodeToPath(newKid, state.StaticMaps.WallMap);
                            if (path == null)
                            {
                                state.Command.Debug?.Raise(this, SolverDebug.FalseSolution, newKid);
                            }
                            else
                            {
                                // Solution
                                state.SolutionsNodes ??= new List<SolverNode>();
                                state.SolutionsNodes.Add(newKid);

                                state.Solutions ??= new List<Path>();
                                state.Solutions.Add(path);
                                
                                state.Command.Debug?.Raise(this, SolverDebug.Solution, newKid);
                                
                                foreach (var n in newKid.PathToRoot())
                                    n.Status = SolverNodeStatus.SolutionPath;
                                newKid.Status = SolverNodeStatus.Solution;

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

     

        private bool NewSolutionChain(SolverState state, SolverNode fwdNode, SolverNode revNode)
        {
            state.SolutionsChains ??= new List<SolutionChain>();
            
            // Check solution
            var potential = SolverHelper.CheckSolutionChain(state, fwdNode, revNode); 
            if (potential != null)
            {
                foreach (var n in fwdNode.PathToRoot().Union(revNode.PathToRoot()))
                    n.Status = SolverNodeStatus.SolutionPath;
                fwdNode.Status = SolverNodeStatus.Solution;
                revNode.Status = SolverNodeStatus.Solution;
                
                var pair = new SolutionChain
                {
                    ForwardNode = fwdNode,
                    ReverseNode = revNode,
                    FoundUsing  = this,
                    Path = potential
                };
                state.SolutionsChains.Add(pair);

                state.Command.Debug.Raise(this, SolverDebug.Solution, pair);
                return true;
            }

            return false;
        }
    }
}