using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public abstract class NodeEvaluator : INodeEvaluator
    {
        protected readonly ISolverNodePoolingFactory nodePoolingFactory;

        protected NodeEvaluator(ISolverNodePoolingFactory nodePoolingFactory)
        {
            this.nodePoolingFactory = nodePoolingFactory;
        }

        public bool SafeMode { get; set; } = true;
        public bool SafeModeThrows { get; set; } = false;
        
        public abstract SolverNode Init(Puzzle puzzle, ISolverQueue queue);
        public abstract bool Evaluate(SolverState state, ISolverQueue queue, INodeLookup pool, INodeLookup? solutionPool, SolverNode node);
      
        protected SolverNode? ConfirmDupLookup(SolverState solverState, INodeLookup pool, SolverNode node, List<SolverNode> toEnqueue, SolverNode newKid)
        {
            if (SafeMode)
            {
                var root = node.Root();
                foreach (var nn in root.Recurse())
                {
                    if (nn.Equals(newKid))
                    {
                        if (nn.CompareTo(newKid) != 0) throw new InvalidOperationException();

                        var sizes = $"Tree:{root.CountRecursive()} vs. Pool:{pool.Statistics.TotalNodes}";

                        var shouldExist                     = pool.FindMatch(nn);
                        var shoudNotBeFound_ButWeWantItToBe = pool.FindMatch(newKid);
                        var message =
                            $"[BAD-DUP] {sizes} " +
                            $"Dup:{toEnqueue.Count()}: ({nn}; pool={shouldExist}) <-> ({newKid}) != {shoudNotBeFound_ButWeWantItToBe} [{pool.TypeDescriptor}]";

                        solverState.Command.Report?.WriteLine(message);
                        
                        if (SafeModeThrows)
                        {
                            throw new Exception(message);    
                        }
                        
                        return nn;
                    }
                }

                return null;
            }
            return null;
        }
    }
}