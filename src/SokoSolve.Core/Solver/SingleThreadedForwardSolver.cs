namespace SokoSolve.Core.Solver
{
    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver() : base(new ForwardEvaluator())
        {
        }
    }
}