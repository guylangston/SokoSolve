using System;

namespace SokoSolve.Core.Solver
{
    public class ConsoleProgressNotifier : IProgressNotifier
    {
        public void Update(ISolver caller, SolverCommandResult state, SolverStatistics global)
        {
            if (global != null)
            {
                var d = global.DurationInSec;

                Console.WriteLine(
                    "\t{0:#,###,##0} nodes at {1:0.0} nodes/sec after {2:#} sec. Depth: {3}/{4}/{5} (Completed/Curr/Max)",
                    global.TotalNodes, global.TotalNodes / d, d, global.DepthCompleted, global.DepthCurrent,
                    global.DepthMax);
            }
        }
    }
}