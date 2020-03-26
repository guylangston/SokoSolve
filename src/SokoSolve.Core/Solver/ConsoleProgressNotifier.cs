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

                Console.CursorLeft = 0;
                Console.Write(
                    $"\t{global.TotalNodes:#,###,##0} nodes at {global.TotalNodes / d:0.0} nodes/sec after {d:#} sec. Depth: {global.DepthCompleted}/{global.DepthCurrent}/{global.DepthMax} (Completed/Curr/Max)"
                        .PadRight(Console.WindowWidth-10));
            }
        }
    }
}