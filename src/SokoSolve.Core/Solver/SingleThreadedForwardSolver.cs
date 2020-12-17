using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory) 
            : base(new ForwardEvaluator(cmd, nodePoolingFactory))
        {
        }

        
        
    }
}