using System;
using System.Collections.Generic;
using System.Linq;

namespace SokoSolve.Core.Solver
{
    public abstract class NodeEvaluator : INodeEvaluator
    {
        protected readonly ISolverNodeFactory nodeFactory;

        protected NodeEvaluator(ISolverNodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        public bool SafeMode { get; set; } = true;
        
        public abstract SolverNode Init(Puzzle puzzle, ISolverQueue queue);
        public abstract bool Evaluate(SolverState state, ISolverQueue queue, ISolverPool pool, ISolverPool solutionPool, SolverNode node);
        
        protected SolverNode? ConfirmDupLookup(ISolverPool pool, SolverNode node, List<SolverNode> toEnqueue, SolverNode newKid)
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
                            $"This is an indication the Pool is not threadsafe/or has a bad binarySearch\n" +
                            $"{sizes}\n" +
                            $"Dup:{toEnqueue.Count()}: ({nn}; pool={shouldExist}) <-> ({newKid}) != {shoudNotBeFound_ButWeWantItToBe} [{pool.TypeDescriptor}]";
                        //throw new Exception(message);
                        return nn;
                    }
                }

                return null;
            }
            return null;
        }
    }
}