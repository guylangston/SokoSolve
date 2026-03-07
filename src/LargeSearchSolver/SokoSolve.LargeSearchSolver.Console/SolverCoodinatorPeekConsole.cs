using SokoSolve.LargeSearchSolver.Lookup;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.Console;

using Console=System.Console;

public class SolverCoodinatorPeekConsole : ISolverCoodinatorPeek
{
    public int PeekEvery { get; set; } = 10_000;

    int lastBackLog;
    int maxBackLog;
    int? goalNode = null;

    public bool TickUpdate(LSolverState state, int totalNodes)
    {
        if (goalNode == null)
        {
            if (state.Request.PuzzleIdent is {} pi)
            {
                if (KnownSolutions.TrueSize.FirstOrDefault(x=>x.PuzzleIdent == pi) is {} match && match.TotalNodesSolution.HasValue)
                {
                    goalNode = (int)match.TotalNodesSolution.Value;
                }
                else
                {
                    goalNode = -1;
                }
            }
            else
            {
                goalNode = -1;
            }
        }
        Console.CursorLeft = 0;

        Console.Write($">> Eval:{totalNodes:#,##0} Heap:{state.Heap.Count:#,##0}");
        if (goalNode > 0)
        {
            Console.Write($"[Gl {(float)state.Heap.Count * 100 / goalNode:0}%] ");
        }
        Console.Write(" Backlog:");
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
        var backlogPerc = (float)bc * 100f / maxBackLog;
        Console.Write($"~{backlogPerc:0}% ");
        Console.ForegroundColor  = cr;
        if (state.Lookup is ILNodeLookupStats lookupStats)
        {
            Console.Write($" Lookup({lookupStats.LookupsTotal/1_000_000f:0.0}mil count:{lookupStats.Count:#,##0} colz:{lookupStats.Collisons})");
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
            if (k.KeyChar == '?')
            {
                InlineReport(state);
            }
        }

        return true;
    }

    private void InlineReport(LSolverState state)
    {
        Console.WriteLine();
        if (state.Lookup is ILNodeLookupNested nested)
        {
            InlineReport(state, nested, 0);
        }
    }

    private void InlineReport(LSolverState state, ILNodeLookupNested nested, int depth)
    {
        foreach(var n in nested.GetNested())
        {
            for(int cc=0; cc<depth; cc++)
                Console.Write("   ");

            var size = "(?)";
            if (n.Inner is ILNodeLookupStats s)
            {
                size = $"Lu:{s.LookupsTotal},Sz:{s.Count},Clz:{s.Collisons}";
            }
            var ll = LookupHelper.Descibe(n.Inner);
            Console.WriteLine($"    --> {n.Desc} ==> {ll.Name}({ll.Desc})    {size}");
            if (n.Inner is ILNodeLookupNested next)
            {
                InlineReport(state, next, depth+1);
            }
        }
    }

    public void Finished()
    {
        Console.CursorLeft = 0;
        Console.WriteLine();
    }
}



