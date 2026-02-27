using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.Console;

using Console=System.Console;

public class SolverCoodinatorPeekConsole : ISolverCoodinatorPeek
{
    public int PeekEvery { get; set; } = 10_000;

    int lastBackLog;
    int maxBackLog;

    public bool TickUpdate(LSolverState state, int totalNodes)
    {
        Console.CursorLeft = 0;

        Console.Write($">> Eval:{totalNodes:#,##0} Heap:{state.Heap.Count:#,##0} Backlog:");
        var bc = state.Backlog.Count;
        var bd = bc - lastBackLog;
        lastBackLog = bc;
        var cr = Console.ForegroundColor;
        var txtClr = bd > 0 ?  ConsoleColor.Blue : ConsoleColor.Green;
        if (bc >= maxBackLog)
        {
            maxBackLog = bc;
            txtClr = ConsoleColor.Red;
        }
        Console.ForegroundColor = txtClr;
        Console.Write(bc.ToString("#,##0").PadLeft(10));
        Console.Write($"({bd,5})");
        Console.ForegroundColor  = cr;

        var backlogPerc = (float)bc * 100f / maxBackLog;
        Console.Write($" {backlogPerc:0}% ");
        if (state.Lookup is ILNodeLookupStats lookupStats)
        {
            Console.Write($" Lookup({lookupStats.LookupsTotal/1_000_000f:0.0}mil count:{lookupStats.Count:#,##0} col:{lookupStats.Collisons})");
        }

        var elapsed = DateTime.Now - state.Started;
        Console.Write($" {Humanize.TimeSpan(elapsed)}");
        Console.Write("   ");

        if (Console.KeyAvailable)
        {
            var k = Console.ReadKey();
            if (k.Key == ConsoleKey.Escape)
            {
                ConsoleSolver.StopRun = true;
                return false;
            }
        }

        return true;
    }
}



