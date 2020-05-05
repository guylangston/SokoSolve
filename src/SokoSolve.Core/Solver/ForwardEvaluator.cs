using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public interface INodeEvaluator
    {
        SolverNode Init(Puzzle puzzle, ISolverQueue queue);

        bool Evaluate(
            SolverState state, 
            ISolverQueue queue,
            ISolverPool pool,
            ISolverPool solutionPool, 
            SolverNode node);
    }

    public class ForwardEvaluator : INodeEvaluator
    {
        private readonly ISolverNodeFactory nodeFactory;

        public ForwardEvaluator(ISolverNodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        public bool SafeMode { get; set; } = true;

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

        public SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var root = CreateRoot(puzzle);
            queue.Enqueue(root);
            return root;
        }

        public bool Evaluate(
            SolverState state, 
            ISolverQueue queue, 
            ISolverPool pool,
            ISolverPool solutionPool, 
            SolverNode node)
        {
            if (node.HasChildren) throw new InvalidOperationException();
            
            
            node.Status = SolverNodeStatus.Evaluting;
            var toEnqueue = new List<SolverNode>();        // TODO: Could be reused
            var toPool = new List<SolverNode>(); // TODO: Could be reused

            var solution = false;
            foreach (var move in node.MoveMap.TruePositions())
            foreach (var dir in VectorInt2.Directions)
            {
                var p = move;
                var pp = p + dir;
                var ppp = pp + dir;
                if (node.CrateMap[pp]                                        // crate to push
                    && state.StaticMaps.FloorMap[ppp] && !node.CrateMap[ppp] // into free space?
                    && !state.StaticMaps.DeadMap[ppp])                       // Valid Push
                {
                    EvaluateValidPush(state, pool, solutionPool, node, pp, ppp, p, dir, toEnqueue, toPool, ref solution);
                }
            }
            
            if (node.SolverNodeId == 2)
            {
                foreach (var kid in node.Children)
                {
                    state.Command.Report.WriteLine($"{kid.SolverNodeId} -> {kid.PlayerBefore} -> {kid.Push}");    
                }
                
            }
            
            if (solution)
            {
                node.Status = SolverNodeStatus.SolutionPath;
            }
            else if (node.HasChildren)
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
            ISolverPool                            pool,
            ISolverPool                            reversePool,
            SolverNode                             node,
            VectorInt2                             pp,
            VectorInt2                             ppp,
            VectorInt2                             p,
            VectorInt2                             push,
            List<SolverNode>                       toEnqueue,
            List<SolverNode>                       toPool,
            ref bool                               solution)
        {
            
            state.Statistics.TotalNodes++;
            
            var newKid = nodeFactory.CreateFromPush(node, node.CrateMap, state.StaticMaps.WallMap, p, pp, ppp, push);

            if (state.Command.Inspector != null && state.Command.Inspector(newKid))
            {
                state.Command.Report?.WriteLine(newKid);
            }

            // Cycle Check: Does this node exist already?
            var dup = pool.FindMatch(newKid);
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
                    nodeFactory.ReturnInstance(newKid); // Add to pool for later re-use?
                }
                else // DuplicateMode.Discard
                {
                    
                }
                
            }
            else
            {
                if (SafeMode)
                {
                    var root = node.Root();
                    foreach (var nn in root.Recurse())
                    {
                        if (nn.Equals(newKid))
                        {
                            if (nn.CompareTo(newKid) != 0) throw new InvalidOperationException();

                            var sizes = $"TreeSize:{root.CountRecursive()} vs. Pool:{pool.Statistics.TotalNodes}";
                            
                            var shouldExist = pool.FindMatch(nn);
                            var shoudNotBeFound_ButWeWantItToBe = pool.FindMatch(newKid);
                            throw new Exception($"{sizes}\n Dup5: ({nn}; pool={shouldExist}) <-> ({newKid}) != {shoudNotBeFound_ButWeWantItToBe} [{pool.TypeDescriptor}]");
                        }
                    }
                }
                
                node.Add(newKid);
                toPool.Add(newKid);
                
                // If there is a reverse solver, checks its pool for a match, hence a Forward <-> Reverse chain, hence a solution
                var match = reversePool?.FindMatch(newKid);
                if (match != null)
                {
                    // Solution!
                    NewSolutionChain(state, out solution, newKid, match);

                    if (state.Command.ExitConditions.StopOnSolution) return true;
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
                            // Solution
                            solution = true;
                            state.SolutionsNodes.Add(newKid);
                            state.Command.Debug.Raise(this, SolverDebug.Solution, newKid);
                            foreach (var n in newKid.PathToRoot())
                                n.Status = SolverNodeStatus.SolutionPath;
                            newKid.Status = SolverNodeStatus.Solution;
                            
                            if (state.Command.ExitConditions.StopOnSolution) return true;
                        }
                    }
                }
            }

            return false;
        }

        private void NewSolutionChain(SolverState state, out bool solution, SolverNode newKid, SolverNode match)
        {
            solution                    =   true;
            state.SolutionsNodesReverse ??= new List<SolutionChain>();
            var pair = new SolutionChain
            {
                ForwardNode = newKid,
                ReverseNode = match,
                FoundUsing  = this
            };
            state.SolutionsNodesReverse.Add(pair);

            state.Command.Debug.Raise(this, SolverDebug.Solution, pair);

            foreach (var n in newKid.PathToRoot().Union(match.PathToRoot()))
                n.Status = SolverNodeStatus.SolutionPath;
            newKid.Status = SolverNodeStatus.Solution;
            match.Status  = SolverNodeStatus.Solution;
        }
    }
}