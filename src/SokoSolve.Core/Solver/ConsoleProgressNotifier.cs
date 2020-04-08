using System;
using System.Data.Common;

namespace SokoSolve.Core.Solver
{
    public class ConsoleProgressNotifier : IProgressNotifier, IDisposable
    {
        DateTime last = DateTime.MinValue;
        private int line;
        private string lastTxt;
        private SolverStatistics prev;

        public ConsoleProgressNotifier()
        {
            
        }

        

        public void Update(ISolver caller, SolverResult state, SolverStatistics global, string txt)
        {
            if (global == null) return;
            

            var dt = DateTime.Now - last;
            if (dt.TotalSeconds < 0.5)
            {
                return;
            }
            
            last = DateTime.Now;

            lastTxt = txt;
            line = Console.CursorTop;
            Console.Write($"{txt}, delta:{global.TotalNodes - (prev?.TotalNodes ?? 0)}".PadRight(Console.WindowWidth-1));
            Console.SetCursorPosition(0, line);
            
            prev = new SolverStatistics(global);
        }

        public void Dispose()
        {
            Console.SetCursorPosition(0, line);
            Console.Write("".PadRight(Console.WindowWidth-1, ' ' ));
            Console.SetCursorPosition(0, line);

        }
    }
}