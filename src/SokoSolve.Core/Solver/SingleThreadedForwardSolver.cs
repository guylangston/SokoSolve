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


        public override ExitResult Solve(SolverBaseState state)
        {
            base.Solve(state);
            if (state.Exit == ExitResult.QueueEmpty) state.Exit = ExitResult.ExhaustedTree;
            return state.Exit;
        }


    }
}