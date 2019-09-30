using System;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.PuzzleLogic;

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