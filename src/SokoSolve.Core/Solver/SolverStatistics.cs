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

        public int TotalNodes     { get; set; }
        public int TotalDead      { get; set; }
        public int DepthCompleted { get; set; }
        public int DepthMax       { get; set; }
        public int DepthCurrent   { get; set; }
        public int Duplicates     { get; set; }

        public DateTime Started       { get; set; }
        public DateTime Completed     { get; set; }
        public TimeSpan Elapased      => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
        public double   DurationInSec => Elapased.TotalSeconds;

        // Control
        public string Name { get; set; }
        public string Text { get; set; }

        public string ToStringShort() =>
            $"[{Name}] {TotalDead}:{TotalNodes:#,##0} @ {TotalNodes / DurationInSec:0}/s ";

        public override string ToString() => 
            $"{Name}: {TotalNodes:#,##0} nodes at {TotalNodes / DurationInSec:0.0} nodes/sec, Duration: {Elapased.ToString("d\\.hh\\:mm\\:ss")}.";
    }
}