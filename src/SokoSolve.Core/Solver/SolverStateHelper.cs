using System;
using SokoSolve.Core.Solver.Solvers;

namespace SokoSolve.Core.Solver
{
    public static class SolverStateHelper
    {
        public static (TreeStateCore? fwd, TreeStateCore? rev) GetTreeState(SolverState state)
        {
            if (state is SolverStateDoubleTree mt)
            {
                return (mt.Forward, mt.Reverse);
            }

            if (state is SolverStateSingleTree single)
            {
                if (single.Solver is SingleThreadedForwardSolver fwd)
                {
                    return (single.TreeState, null);
                }
                if (single.Solver is SingleThreadedReverseSolver rev)
                {
                    return (null, single.TreeState);
                }
            }

            throw new NotImplementedException(state.GetType().Name);
        }
    }
}
