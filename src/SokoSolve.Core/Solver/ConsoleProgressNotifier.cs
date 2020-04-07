using System;
using System.Data.Common;

namespace SokoSolve.Core.Solver
{
    public class ConsoleProgressNotifier : IProgressNotifier
    {
        DateTime last = DateTime.MinValue;

        public ConsoleProgressNotifier()
        {
            
        }

        public void Update(ISolver caller, SolverResult state, SolverStatistics global)
        {
            if (global == null) return;

            var dt = DateTime.Now - last;
            if (dt.TotalSeconds < 5)
            {
                return;
            }
            
            last = DateTime.Now;
            
            Console.WriteLine($"-> {global}");
        }
    }
}