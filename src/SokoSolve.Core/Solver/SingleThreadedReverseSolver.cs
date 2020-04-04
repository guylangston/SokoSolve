using System.Collections.Generic;
using SokoSolve.Core.Primitives;
using VectorInt;

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
            public SyntheticReverseNode(VectorInt2 playerBefore, VectorInt2 playerAfter, VectorInt2 crateBefore, VectorInt2 crateAfter, Bitmap crateMap, Bitmap moveMap, int goals, INodeEvaluator? evaluator) : base(playerBefore, playerAfter, crateBefore, crateAfter, crateMap, moveMap, goals, evaluator)
            {
            }
        }

        

        public override IEnumerable<(string name, string text)> GetSolverDescriptionProps(SolverCommandResult state)
        {
            throw new System.NotImplementedException();
        }
    }
}