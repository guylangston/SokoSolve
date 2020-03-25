using System;
using System.Collections.Generic;
using SokoSolve.Core.Game;

namespace SokoSolve.Core.Lib.DB
{
    public class PuzzleDTO
    {
        public int PuzzleId { get; set; }
        public string Name { get; set; }
        public int Hash { get; set; }
        public int Rating { get; set; }
        public string CharMap { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }

        public string Author { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }

        public string SourceIdent { get; set; }

        public List<SolutionDTO> Solutions { get; set; }
    }

    public class SolutionDTO
    {
        public int      SolutionId         { get; set; }
        public string   PuzzleIdent        { get; set; }
        public string   Path               { get; set; }
        
        public string Author      { get; set; }
        public string URL         { get; set; }
        public string Email       { get; set; }
        public string Description { get; set; }

        public bool     IsAutomated        { get; set; }
        public string   HostMachine        { get; set; }
        public int      TotalNodes         { get; set; }
        public double   TotalSecs          { get; set; }
        public string   SolverType         { get; set; }
        public int      SolverVersionMajor { get; set; }
        public int      SolverVersionMinor { get; set; }
        public string   SolverDescription  { get; set; }
        public DateTime Created            { get; set; }
        public DateTime Modified           { get; set; }
        public string   Report             { get; set; }
    }

    public interface ISokobanRepository
    {
        LibraryPuzzle GetPuzzle(PuzzleIdent puzzleId);
        IReadOnlyCollection<SolutionDTO> GetSolutions(PuzzleIdent puzzleId);

        // Writes
        void Store(SolutionDTO sol);
        void Update(SolutionDTO sol);
    }

}