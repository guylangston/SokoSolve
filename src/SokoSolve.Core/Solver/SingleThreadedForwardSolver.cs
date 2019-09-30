using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SokoSolve.Core.PuzzleLogic;

namespace SokoSolve.Core.Solver
{


    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver() : base(new ForwardEvaluator())
        {
        }
    }
}

