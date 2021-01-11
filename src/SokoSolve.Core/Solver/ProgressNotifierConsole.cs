using System;
using System.IO;
using SokoSolve.Core.Common;
using TextRenderZ;
using TextRenderZ.Reporting;

namespace SokoSolve.Core.Solver
{
    public abstract class ProgressNotifierSampling : IProgressNotifier
    {
        DateTime last = DateTime.MinValue;
        private double sampleRateInSec = 0.5;
        
        public string LastUpdate { get; protected set; }
        
        public void Update(ISolver caller, SolverState state, SolverStatistics global, string txt)
        {
            var dt = DateTime.Now - last;
            if (dt.TotalSeconds < sampleRateInSec)
            {
                return;
            }
            last       = DateTime.Now;
            LastUpdate = txt;

            UpdateInner(caller, state, global, txt);
        }

        protected abstract void UpdateInner(ISolver caller, SolverState state, SolverStatistics global, string txt);
    }

    public abstract class ProgressNotifierSamplingMulticast : ProgressNotifierSampling
    {
        private TextWriter a;
        private TextWriter b;

        protected ProgressNotifierSamplingMulticast(TextWriter a, TextWriter b)
        {
            this.a = a;
            this.b = b;
        }

        protected abstract string Render(ISolver caller, SolverState state, SolverStatistics global, string txt);

        protected override void UpdateInner(ISolver caller, SolverState state, SolverStatistics global, string txt)
        {
            var l = Render(caller, state, global, txt);
            a.WriteLine(l);
            b.WriteLine(l);
        }
    }
    
    public abstract class ProgressNotifierSamplingMulticastConsole : ProgressNotifierSampling, IDisposable
    {
        private TextWriter a;
        private int line;
        private int lineWin;
        private bool supported;

        protected ProgressNotifierSamplingMulticastConsole(TextWriter a)
        {
            this.a = a;
            
            try
            {
                //System.Console.WindowTop = System.Console.WindowTop;
                System.Console.CursorTop = System.Console.CursorTop;
                supported = true;
            }
            catch (Exception e)
            {
                supported = false;
            }
        }

        protected abstract string Render(ISolver caller, SolverState state, SolverStatistics global, string txt);

        protected override void UpdateInner(ISolver caller, SolverState state, SolverStatistics global, string txt)
        {
            var l = Render(caller, state, global, txt);
            
            a.WriteLine(l);
            UpdateConsoleInPlace(l);
        }

        private void UpdateConsoleInPlace(string l)
        {
            if (supported)
            {
                var max = System.Console.WindowWidth - 2;
                //lineWin = System.Console.WindowTop;
                line    = System.Console.CursorTop;
                System.Console.Write(StringUtil.Truncate(l, max).PadRight(max));
                //System.Console.WindowTop = lineWin;
                System.Console.SetCursorPosition(0, line);
            }
            else
            {
                System.Console.WriteLine(l);
            }
        }

        public void Dispose()
        {
            UpdateConsoleInPlace("");
        }

    }
    
    
    public class ConsoleProgressNotifier : ProgressNotifierSamplingMulticastConsole
    {
        private readonly ITextWriterAdapter tele;
        private SolverStatistics? prev;

        public ConsoleProgressNotifier(ITextWriterAdapter tele) 
            : base(TextWriter.Null) // we want a different format to go to file
        {
            this.tele = tele;
            
            var telText = new FluentString(",")
                          .Append("DurationInSec").Sep()
                          .Append("TotalNodes").Sep()
                          .Append("NodesPerSec").Sep()
                          .Append("NodesDelta").Sep()
                          .Append("MemoryUsed")
                ;
            
            tele.WriteLine(telText);
            
        }
        
        protected override string Render(ISolver caller, SolverState state, SolverStatistics global, string txt)
        {
            if (global == null) return null;
            
            var totalMemory = System.GC.GetTotalMemory(false);

            var delta = (prev != null) ? global.TotalNodes - prev.TotalNodes : global.TotalNodes;

            var sb = new FluentString(" ")
                .Append(txt)
                .Sep()
                .Append($"delta:{delta:#,##0}")
                .Sep()
                .Append($"mem({Humanise.SizeSuffix((ulong) totalMemory)} used")
                .Block(b =>
                {
                    if (DevHelper.TryGetTotalMemory(out var avail))
                    {
                        b.Sep();
                        b.Append($"{Humanise.SizeSuffix(avail)} avail");
                    }
                })
                .Append(")");
            
            
            var telText = new FluentString(",")
                    .Append(global.DurationInSec.ToString()).Sep()
                    .Append(global.TotalNodes.ToString()).Sep()
                    .Append(global.NodesPerSec.ToString()).Sep()
                    .Append(delta.ToString()).Sep()
                    .Append(totalMemory.ToString()).Sep()
                ;
            
            tele.WriteLine(telText);
            
            prev = new SolverStatistics(global);
            
            
            
            
            return sb;
        }

      

        
        
       

        
    }
}