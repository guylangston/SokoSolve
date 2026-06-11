using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Lib;

namespace SokoSolve.Core.Solver
{
    public class SolverRun : List<LibraryPuzzle>
    {
        public ExitConditions? BatchExit { get; set; }
        public ExitConditions? PuzzleExit { get; set; }

        public void Load(LibraryComponent lib, string fileName)
        {
            Init();
            var batchExit   = BatchExit   ?? throw new InvalidOperationException("Init failed to set BatchExit");
            var puzzleExit  = PuzzleExit  ?? throw new InvalidOperationException("Init failed to set PuzzleExit");

            var idents = new List<PuzzleIdent>();
            var file = TrivialNameValueFileFormat.Load(lib.GetPathData(fileName));
            foreach (var pair in file)
            {
                if (pair.Key == "PuzzleExit.Duration") puzzleExit.Duration = TimeSpan.FromSeconds(int.Parse(pair.Value));

                if (pair.Key == "BatchExit.Duration") batchExit.Duration = TimeSpan.FromSeconds(int.Parse(pair.Value));

                if (pair.Key.StartsWith("Puzzle.")) idents.Add(PuzzleIdent.Parse(pair.Value));
            }

            AddRange(lib.LoadAllPuzzles(idents));
        }

        public void Load(Lib.Library lib)
        {
            Init();

            AddRange(lib.OrderBy(x=>x.Rating));
        }

        public void Load(IEnumerable<LibraryPuzzle> lib)
        {
            Init();

            AddRange(lib.OrderBy(x=>x.Rating));
        }

        public void Init()
        {
            PuzzleExit = ExitConditions.Default3Min();
            BatchExit = new ExitConditions
            {
                Duration   = TimeSpan.FromHours(8),
                MaxNodes = int.MaxValue,
                MaxDead  = int.MaxValue
            };
        }
    }
}
