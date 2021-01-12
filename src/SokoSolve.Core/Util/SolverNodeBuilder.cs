using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.NodeFactory;
using SokoSolve.Core.Solver.Solvers;

namespace SokoSolve.Core.Util
{
    public static class SolverNodeBuilder
    {
        public static IEnumerable<SolverNode> BuildSolverNodes(Puzzle puzzle, int count)
        {
            var exit = new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(10),
                StopOnSolution = false,
                MaxNodes       = count,
                MaxDead        = int.MaxValue
            };
            
            var container = new SolverContainerByType();
            var command   = new SolverCommand(puzzle, PuzzleIdent.Temp(), exit, container);
            var solver    = new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault());
            
            var result = (SolverStateMultiThreaded)solver.Init(command);
            solver.Solve(result);
            result.ThrowErrors();

            return Enumerable.Union(result.Forward.Root.Recurse(), result.Reverse.Root.Recurse());
        }

        public static IEnumerable<SolverNode> BuildSolverNodes(int count) => BuildSolverNodes(Puzzle.Builder.Reference_UnSolved_SQ1P13(), count);
    }
}