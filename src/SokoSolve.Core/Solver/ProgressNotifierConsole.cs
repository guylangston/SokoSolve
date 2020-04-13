using System;
using System.Data.Common;
using System.IO;
using System.Runtime.InteropServices;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public abstract class ProgressNotifierSampling : IProgressNotifier
    {
        DateTime last = DateTime.MinValue;
        private double sampleRateInSec = 0.5;
        
        public void Update(ISolver caller, SolverResult state, SolverStatistics global, string txt)
        {
            var dt = DateTime.Now - last;
            if (dt.TotalSeconds < sampleRateInSec)
            {
                return;
            }
            last = DateTime.Now;

            UpdateInner(caller, state, global, txt);
        }

        protected abstract void UpdateInner(ISolver caller, SolverResult state, SolverStatistics global, string txt);
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

        protected abstract string Render(ISolver caller, SolverResult state, SolverStatistics global, string txt);

        protected override void UpdateInner(ISolver caller, SolverResult state, SolverStatistics global, string txt)
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

        protected abstract string Render(ISolver caller, SolverResult state, SolverStatistics global, string txt);

        protected override void UpdateInner(ISolver caller, SolverResult state, SolverStatistics global, string txt)
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
                System.Console.Write(StringHelper.Truncate(l, max).PadRight(max));
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
        private SolverStatistics prev;

        public ConsoleProgressNotifier(TextWriter a) : base(a)
        {
        }

        protected override string Render(ISolver caller, SolverResult state, SolverStatistics global, string txt)
        {
            if (global == null) return null;
            
            var totalMemory = System.GC.GetTotalMemory(false);

            var sb = new FluentStringBuilder()
                .Append(txt)
                .Sep()
                .Append($"mem({StringHelper.SizeSuffix((ulong) totalMemory)} used");
            
            try
            {
                var memoryStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memoryStatus))
                {
                    sb.Sep();
                    sb.Append($"{StringHelper.SizeSuffix(memoryStatus.ullAvailPhys)} avail");
                    
                }
            }
            catch (Exception)
            {
                
            }

            sb.Append(")");
            
            prev = new SolverStatistics(global);
            return sb;
        }

      

        
        
        [StructLayout(LayoutKind.Sequential, CharSet =CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint  dwLength;
            public uint  dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint) Marshal.SizeOf(typeof( MEMORYSTATUSEX ));
            }
        }


        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool GlobalMemoryStatusEx( [In, Out] MEMORYSTATUSEX lpBuffer);
    }
}