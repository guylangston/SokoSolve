using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Core.Solver
{


    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver() : base(new ForwardEvaluator())
        {
        }
    }
}

