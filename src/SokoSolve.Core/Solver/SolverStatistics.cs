using System;
using System.IO;
using SokoSolve.Core.Common;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public class SolverStatistics
    {
        public SolverStatistics()
        {
            Started   = DateTime.Now;
            Completed = DateTime.MinValue;
        }

        public SolverStatistics(SolverStatistics copy)
        {
            this.TotalNodes     = copy.TotalNodes;
            this.TotalDead      = copy.TotalDead;
            this.DepthCompleted = copy.DepthCompleted;
            this.DepthMax       = copy.DepthMax;
            this.DepthCurrent   = copy.DepthCurrent;
            this.Duplicates     = copy.Duplicates;
            this.Started        = copy.Started;
            this.Completed      = copy.Completed;
            this.Name           = copy.Name;
            this.Text           = copy.Text;
            this.Type           = copy.Type;
            this.MemUsed = copy.MemUsed;
        }

        public int  CurrentNodes   { get; set; } = -1;
        public int  TotalNodes     { get; set; } = -1;
        public int  TotalDead      { get; set; } = -1;
        public int  DepthCompleted { get; set; } = -1;
        public int  DepthMax       { get; set; } = -1;
        public int  DepthCurrent   { get; set; } = -1;
        public int  Duplicates     { get; set; } = -1;
        public long MemUsed        { get; set; } = -1;

        public DateTime Started        { get; set; }
        public DateTime Completed      { get; set; }
        public TimeSpan Elapsed        => (Completed == DateTime.MinValue ? DateTime.Now : Completed) - Started;
        public double   DurationInSec  => Elapsed.TotalSeconds;
        public double   NodesPerSec    => ((double) TotalNodes) / DurationInSec;
        
        public string? Name     { get; set; }
        public string? Text     { get; set; }
        public string? Type     { get; set; }
        
        public int     Warnings { get; set; }
        public int     Errors   { get; set; }


        public string ToString(bool verbose, bool skipName = false)
        {
            var builder = new FluentString()
                .If(Name != null && !skipName, $"{Name,-40}");
            
            if (DurationInSec <= 0d || TotalNodes <= 0)
            {
                return builder.Append($"{TotalNodes,12:#,##0} nodes");
            }

            builder.When(verbose, then => 
               then.If(CurrentNodes >= 0, () => $" Nodes={CurrentNodes:#,##0}")
                   .If(TotalDead >= 0, () => $" Dead={TotalDead:#,##0}:{TotalDead * 100 / TotalNodes:0}%")
                   .If(Duplicates >= 0, () => $" Dup={Duplicates:#,##0}:{Duplicates * 100 / TotalNodes:0}%")
                   .If(DepthCurrent >= 0, () => $" Depth={DepthCurrent:#,##0}:{DepthMax:#,##0}")
                   .If(DepthCompleted >= 0, () => $" DepthComplete={DepthCompleted:#,##0}")
                   .If(Warnings >= 0, () => $" Warnings={Warnings:#,##0}")
                   .If(Errors >= 0, () => $" Errors={Errors:#,##0}")
                   )
            .Append($"{TotalNodes,11:#,##0} nodes at {TotalNodes / DurationInSec,7:#,##0}/s in {Elapsed.Humanize()}");

            return builder;
        }

        public string ToStringShort() => ToString(false);

        public override string ToString() => ToString(false);
    }
}