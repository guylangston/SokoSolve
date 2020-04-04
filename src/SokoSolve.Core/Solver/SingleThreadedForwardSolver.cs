using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver() : base(new ForwardEvaluator())
        {
        }

        public override IEnumerable<(string name, string text)> GetSolverDescriptionProps(SolverCommandResult state)
        {
            throw new System.NotImplementedException();
        }
    }
}