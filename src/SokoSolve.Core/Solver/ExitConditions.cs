using System;
using System.Text;
using SokoSolve.Core.Common;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public class ExitConditions
    {
        public enum Conditions
        {
            Continue,

            TotalNodes,
            TotalDead,
            TimeOut,
            Solution,
            ExhaustedTree,
            QueueEmpty,
            Aborted,
            Error,
            Memory,
            Stopped
        }

        public ExitConditions()
        {
            TotalNodes = int.MaxValue;
            TotalDead = int.MaxValue;
            MemAvail = 0;
            MemUsed = 0;
            Duration = TimeSpan.FromMinutes(3);
            StopOnSolution = true;
        }

        public int      TotalNodes     { get; set; }
        public int      TotalDead      { get; set; }
        public TimeSpan Duration       { get; set; }
        public bool     StopOnSolution { get; set; }
        public bool     ExitRequested  { get; set; }
        public long    MemAvail { get; set;  }
        public long    MemUsed { get; set;  }
        
        public static ExitConditions OneMinute()  => new ExitConditions
        {
            Duration       = TimeSpan.FromSeconds(60)
        };

        public static ExitConditions Default3Min() => new ExitConditions
        {
            Duration       = TimeSpan.FromMinutes(3),
        };

        public static ExitConditions Default10Min() => new ExitConditions
        {
            Duration       = TimeSpan.FromMinutes(10),
        };

        public Conditions ShouldExit(SolverState state)
        {
            if (ExitRequested) return Conditions.Aborted;
            if (StopOnSolution && state.HasSolution) return Conditions.Solution;
            if (state.Statistics.TotalNodes >= TotalNodes) return Conditions.TotalNodes;
            if (DateTime.Now - state.Statistics.Started >= Duration) return Conditions.TimeOut; // TODO: This is unnessesarily slow
            if (MemUsed != 0 && GC.GetTotalMemory(false) >= MemUsed) return Conditions.Memory;
            if (MemAvail != 0 && DevHelper.TryGetTotalMemory(out var avail) && (long)avail < MemAvail)  return Conditions.Memory;

            if (state is MultiThreadedSolverState multi)
            {
                // Messy logic, as MultiThreaded as a master state and copies as client worker states (both MultiThreadedSolverState)
                if (multi.IsMaster && !multi.IsRunning) return Conditions.Stopped;
                if (!multi.IsMaster && !multi.ParentState.IsRunning) return Conditions.Stopped;
            }

            return Conditions.Continue;
        }

        public override string ToString() =>
            new FluentString(", ")
                .If(TotalNodes != int.MaxValue, $"TotalNodes: {TotalNodes:#,##00}").Sep()
                .If(TotalDead != int.MaxValue,  $"TotalDead: {TotalNodes:#,##00}").Sep()
                .If(MemAvail != 0,  $"MemAvail: {Humanise.SizeSuffix((ulong)MemAvail)}").Sep()
                .If(MemUsed != 0,  $"MemUsed: {Humanise.SizeSuffix((ulong)MemUsed)}").Sep()
                .Append($"TimeOut: {Duration.Humanize()}");
    }
}