using System;
using SokoSolve.Core.Solver;

namespace SokoSolve.Core.Lib.DB
{
    public class SolutionDTO
    {
        public int    SolutionId  { get; set; }
        public string PuzzleIdent { get; set; }
        public string Path        { get; set; }
        public bool HasSolution => !string.IsNullOrWhiteSpace(Path); 
        
        public string Author      { get; set; }
        public string URL         { get; set; }
        public string Email       { get; set; }
        public string Description { get; set; }

        public bool IsAutomated { get; set; } = true;
        public string   MachineName        { get; set; }
        public string MachineCPU { get; set; }
        public int      TotalNodes         { get; set; }
        public double   TotalSecs          { get; set; }
        public string   SolverType         { get; set; }
        public int      SolverVersionMajor { get; set; }
        public int      SolverVersionMinor { get; set; }
        public string   SolverDescription  { get; set; }
        public DateTime Created            { get; set; }
        public DateTime Modified           { get; set; }
        public string   Report             { get; set; }

        protected bool Equals(SolutionDTO other)
        {
            return SolutionId == other.SolutionId && PuzzleIdent == other.PuzzleIdent;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SolutionDTO) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SolutionId, PuzzleIdent);
        }

        public override string ToString()
        {
            return $"{TotalNodes} in {TotalSecs} on {MachineName} => {Path?.Length}";
        }
    }
}