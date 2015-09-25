using System;
using System.Collections.Generic;
using Sokoban.Core.Library;
using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Core.Solver
{
    public class SolverRun : List<Puzzle>
    {
        public ExitConditions BatchExit { get; set; }

        public ExitConditions PuzzleExit { get; set; }

        public void Load(string fileName)
        {
            PuzzleExit = Solver.ExitConditions.Default3Min;
            BatchExit = new ExitConditions()
            {
                Duration = TimeSpan.FromHours(0.5),
                TotalNodes = int.MaxValue,
                TotalDead = int.MaxValue
            };

            var lib = new LibraryComponent();

            var idents = new List<PuzzleIdent>();
            var file = TrivialNameValueFileFormat.Load(lib.GetPathData(fileName));
            foreach (var pair in file)
            {
                
                    if (pair.Key == "PuzzleExit.Duration")
                    {
                        BatchExit.Duration = TimeSpan.FromSeconds(int.Parse(pair.Value));
                    }
                
                    if (pair.Key == "BatchExit.Duration")
                    {
                        BatchExit.Duration = TimeSpan.FromSeconds(int.Parse(pair.Value));
                    }
                
                if (pair.Key.StartsWith("Puzzle."))
                {
                    idents.Add(PuzzleIdent.Parse(pair.Value));
                }
            }

            AddRange(lib.LoadAllPuzzles(idents));
        }
    }
}