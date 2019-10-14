namespace SokoSolve.Core.Solver
{
    public class SingleThreadedReverseSolver : SolverBase
    {
        public SingleThreadedReverseSolver()
            : base(new ReverseEvaluator())
        {
        }


        public class SyntheticReverseNode : SolverNode
        {
        }
    }
}