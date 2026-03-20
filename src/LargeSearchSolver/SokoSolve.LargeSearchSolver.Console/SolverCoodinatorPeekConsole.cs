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

    public bool TickUpdate(LSolverState state, int totalNodes, ref NodeStruct current)
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
            Console.Write($"[Gl {(float)state.Heap.Count * 100 / goalNode:0}%]");
        }
        if (state.Lookup is ILNodeLookupStats lookupStats)
        {
            Console.Write($" Lookup({lookupStats.LookupsTotal/1_000_000f:0.0}mil count:{lookupStats.Count:#,##0} colz:{lookupStats.Collisons})");
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

        // TODO: Move tags into Request (rather than having to know about coord and factory
        if (state.Coordinator is SolverCoordinator sc && sc.StateFactory is SolverCoordinatorFactory sf)
        {
            if (sf.HasTag("MEMORY"))
            {
                var used = GC.GetTotalMemory(false);
                var usedTxt = Humanize.Bytes(used);
                Console.Write($" RAM:{usedTxt}");
                if (state.MemAvailAtStart > 0)
                {
                    float perc = (float)used*100f / (float)state.MemAvailAtStart;
                    Console.Write($"/{Humanize.Bytes(state.MemAvailAtStart)}({perc:0}%)");
                }
                Console.Write($"/{Humanize.Bytes(GC.GetGCMemoryInfo().TotalAvailableMemoryBytes)}");
            }
        }

        var elapsed = DateTime.Now - state.Started;
        Console.Write($" {Humanize.TimeSpan(elapsed)}");
        Console.Write("   ");

        if (Console.KeyAvailable)
        {
            var k = Console.ReadKey();
            if (k.Key == ConsoleKey.Escape || k.KeyChar == 'q')
            {
                ConsoleSolver.StopRun = true;
                return false;
            }
            if (k.KeyChar == ':')
            {
                StartCommandMode(state, ref current);
            }
        }

        return true;
    }

    private void StartCommandMode(LSolverState state, ref NodeStruct current)
    {
        string clearCurrentLine = "\r\u001B[2K";
        Console.Write(clearCurrentLine);
        while(true)
        {
            Console.Write(":");
            var cmd = Console.ReadLine();
            if (cmd == "continue") break;
            if (cmd == "exit" || cmd == "quit")
            {
                state.StopRequested = true;
                break;
            }
            if (cmd == "report lookup")
            {
                InlineReport(state);
                continue;
            }
            if (cmd == "current")
            {
                Console.WriteLine(current.ToDebugString(state));
            }
            Console.WriteLine($"Unknown command: {cmd}");
        }

    }

    private void InlineReport(LSolverState state)
    {
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



