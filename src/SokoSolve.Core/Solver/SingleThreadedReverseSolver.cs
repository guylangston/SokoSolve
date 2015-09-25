using System;
using System.Linq;
using Sokoban.Core.Analytics;
using Sokoban.Core.Primitives;
using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Core.Solver
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