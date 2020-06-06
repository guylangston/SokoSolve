using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver(ISolverNodeFactory nodeFactory) 
            : base(new ForwardEvaluator(nodeFactory))
        {
        }

        
        
    }
}