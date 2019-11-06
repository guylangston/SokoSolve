using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Game;
using SokoSolve.Core.Library;
using SokoSolve.Core.Model.DataModel;

namespace SokoSolve.Core.Solver
{
    public class SolverRun : List<LibraryPuzzle>
    {
        public ExitConditions BatchExit { get; set; }

        public ExitConditions PuzzleExit { get; set; }

        public void Load(LibraryComponent lib, string fileName)
        {
            PuzzleExit = ExitConditions.Default3Min();
            BatchExit = new ExitConditions
            {
                Duration = TimeSpan.FromHours(0.5),
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };

            var idents = new List<PuzzleIdent>();
            var file = TrivialNameValueFileFormat.Load(lib.GetPathData(fileName));
            foreach (var pair in file)
            {
                if (pair.Key == "PuzzleExit.Duration") BatchExit.Duration = TimeSpan.FromSeconds(int.Parse(pair.Value));

                if (pair.Key == "BatchExit.Duration") BatchExit.Duration = TimeSpan.FromSeconds(int.Parse(pair.Value));

                if (pair.Key.StartsWith("Puzzle.")) idents.Add(PuzzleIdent.Parse(pair.Value));
            }

            AddRange(lib.LoadAllPuzzles(idents));
        }

        public void Load(Library.Library lib)
        {
            PuzzleExit = ExitConditions.Default3Min();
            BatchExit = new ExitConditions
            {
                Duration = TimeSpan.FromHours(0.5),
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };

            AddRange(lib);
        }
    }
}