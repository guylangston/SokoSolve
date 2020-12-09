using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver(ISolverNodePoolingFactory nodePoolingFactory) 
            : base(new ForwardEvaluator(nodePoolingFactory))
        {
        }

        
        
    }
}