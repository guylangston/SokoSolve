using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver.ConsoleHost;

public class SolverCoodinatorPeekConsole : ISolverCoodinatorPeek
{
    public int PeekEvery { get; set; } = 10_000;

    int lastBackLog;

    public bool TickUpdate(LSolverState state, int totalNodes)
    {
        Console.CursorLeft = 0;

        Console.Write($">> EvalCount:{totalNodes:#,##0} Heap:{state.Heap.Count:#,##0} BackLog:");
        var bc = state.Backlog.Count;
        var bd = bc - lastBackLog;
        lastBackLog = bc;
        var cr = Console.ForegroundColor;
        Console.ForegroundColor = bd > 0 ?  ConsoleColor.Blue : ConsoleColor.Green;
        Console.Write(bc.ToString("#,##0"));
        Console.Write($"({bd})");
        Console.ForegroundColor  = cr;

        var backlogPerc = (float)bc * 100f / totalNodes;
        Console.Write($" {backlogPerc:0}% ");
        if (state.Lookup is ILNodeLookupStats lookupStats)
        {
            Console.Write($" Lookup({lookupStats.LookupsTotal/1_000_000f:0.0}m, count:{lookupStats.Count:#,##0}, col!:{lookupStats.Collisons})");
        }
        Console.Write("   ");

        if (Console.KeyAvailable)
        {
            var k = Console.ReadKey();
            if (k.Key == ConsoleKey.Escape)
            {
                Program.StopRun = true;
                return false;
            }
        }

        return true;
    }
}



