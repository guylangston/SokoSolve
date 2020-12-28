using System;
using System.Text;
using SokoSolve.Core.Common;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public enum ExitResult
    {
        Continue,

        TotalNodes,
        TotalDead,
        TimeOut,
        Solution,
        ExhaustedTree,
        ExhaustedTreeSolution,
        QueueEmpty,
        Aborted,
        Error,
        Memory,
        Stopped
    }

    public class ExitConditions
    {
        public ExitConditions()
        {
            MaxNodes = int.MaxValue;
            MaxDead = int.MaxValue;
            MemAvail = 0;
            MemUsed = 0;
            Duration = TimeSpan.FromMinutes(3);
            StopOnSolution = true;
        }

        public int      MaxNodes     { get; set; }
        public int      MaxDead      { get; set; }
        public TimeSpan Duration       { get; set; }
        public bool     StopOnSolution { get; set; }
        public bool     ExitRequested  { get; set; }
        public long     MemAvail       { get; set; }
        public long     MemUsed        { get; set; }
        
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

        public ExitResult ShouldExit(SolverState state)
        {
            if (ExitRequested) return ExitResult.Aborted;
            if (StopOnSolution && state.HasSolution) return ExitResult.Solution;
            if (state.GlobalStats.TotalNodes >= MaxNodes) return ExitResult.TotalNodes;
            if (DateTime.Now - state.GlobalStats.Started >= Duration) return ExitResult.TimeOut; // TODO: This is unnessesarily slow
            if (MemUsed != 0 && GC.GetTotalMemory(false) >= MemUsed) return ExitResult.Memory;
            if (MemAvail != 0 && DevHelper.TryGetTotalMemory(out var avail) && (long)avail < MemAvail)  return ExitResult.Memory;

            if (state is SolverStateMultiThreaded multi && !multi.IsRunning) return ExitResult.Stopped;
            if (state is SolverStateMultiThreaded.WorkerState worker && !worker.ParentState.IsRunning) return ExitResult.Stopped;

            return ExitResult.Continue;
        }

        public override string ToString() =>
            new FluentString(", ")
                .If(MaxNodes != int.MaxValue, $"MaxNodes: {MaxNodes:#,##00}").Sep()
                .If(MaxDead != int.MaxValue,  $"MaxDead: {MaxNodes:#,##00}").Sep()
                .If(MemAvail != 0,  $"MemAvail: {Humanise.SizeSuffix((ulong)MemAvail)}").Sep()
                .If(MemUsed != 0,  $"MemUsed: {Humanise.SizeSuffix((ulong)MemUsed)}").Sep()
                .Append($"TimeOut: {Duration.Humanize()}");
    }
}