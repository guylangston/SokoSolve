using System;

namespace SokoSolve.Core.Solver
{
    public class SolverStatistics
    {
        public SolverStatistics()
        {
            Started   = DateTime.Now;
            Completed = DateTime.MinValue;
        }

        public int TotalNodes     { get; set; } = -1;
        public bool IsSample { get; set; }

        public int TotalDead      { get; set; } = -1;
        public int DepthCompleted { get; set; } = -1;
        public int DepthMax       { get; set; } = -1;
        public int DepthCurrent   { get; set; } = -1;
        public int Duplicates     { get; set; } = -1;

        public DateTime Started       { get; set; }
        public DateTime Completed     { get; set; }
        public TimeSpan Elapased      => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
        public double   DurationInSec => Elapased.TotalSeconds;

        // Control
        public string? Name { get; set; }
        public string? Text { get; set; }
        public string? Type { get; set; }

        public string ToStringShort()
        {
            if (TotalDead < 0)
            {
                return $"[{Name,-20}] {TotalNodes,10:#,##0} @ {TotalNodes / DurationInSec:0}/s ";
            }
            return $"[{Name,-20}] {TotalNodes,10:#,##0} @ {TotalNodes / DurationInSec:0}/s D:{TotalDead:#,##0}:{TotalDead * 100 / TotalNodes:0}%";
        }

        public override string ToString() => 
            $"{Name}: {TotalNodes:#,##0} nodes at {TotalNodes / DurationInSec:0.0} nodes/sec, Duration: {Elapased.ToString("d\\.hh\\:mm\\:ss")}.";
    }
}