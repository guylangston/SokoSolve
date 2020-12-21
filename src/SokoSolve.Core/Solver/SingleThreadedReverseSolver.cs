using System;
using System.Collections.Generic;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedReverseSolver : SolverBase
    {
        public SingleThreadedReverseSolver(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory)
            : base(new ReverseEvaluator(cmd, nodePoolingFactory))
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