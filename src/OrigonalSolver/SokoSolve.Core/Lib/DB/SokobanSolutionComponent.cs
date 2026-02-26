using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Solver;

namespace SokoSolve.Core.Lib.DB
{
    public interface ISokobanSolutionComponent
    {
        bool CheckSkip(LibraryPuzzle puzzle, ISolver solver);
        bool StoreIfNecessary(SolverState state);
        bool HasMachineSolution(LibraryPuzzle puzzle);
    }

    public class SokobanSolutionComponent : ISokobanSolutionComponent
    {
        private  readonly ISokobanSolutionRepository repo;

        public SokobanSolutionComponent(ISokobanSolutionRepository repo)
        {
            this.repo = repo;
        }

        public bool SkipIfSolutionExists { get; set; }

        public bool CheckSkip(LibraryPuzzle puzzle, ISolver solver)
        {
            if (SkipIfSolutionExists)
            {
                return GetSolutionsForMachineAndSolver(puzzle.Ident, solver) != null;
            }
            return false;
        }

        public bool StoreIfNecessary(SolverState state)
        {
            if (!state.HasSolution) return false;  // Don't store attempts

            var bestSolution = FindBestSolution(state.Solutions);

            // Basic Rule : One Solution Per Machine Per Solver
            var existing = GetSolutionsForMachineAndSolver(state.Command.PuzzleIdent, state.Solver);
            if (existing is null)
            {
                repo.Store(CreateDTO(state, bestSolution));
                return true;
            }
            if (existing.All(x=>!x.HasSolution))  // no solutions
            {
                // Replace add existing attempts

                foreach (var ee in existing)
                {
                    repo.Remove(ee);
                }

                repo.Store(CreateDTO(state, bestSolution));
                return true;
            }
            else
            {
                foreach (var ee in existing.Where(x=>!x.HasSolution))
                {
                    repo.Remove(ee);
                }

                foreach (var ee in existing.Where(x=>x.HasSolution))
                {
                    if (IsBetter(bestSolution, ee))
                    {
                        var update = CreateDTO(state, bestSolution);
                        update.SolutionId = ee.SolutionId;
                        repo.Store(update);
                        return true;
                    }
                }

            }
            return false;
        }

        public bool HasMachineSolution(LibraryPuzzle puzzle)
        {
            var sols = repo.GetPuzzleSolutions(puzzle.Ident);
            return sols != null && sols.Any(x=>x.HasSolution && x.IsAutomatedCheck);
        }

        private bool IsBetter(Path curr, SolutionDTO ee)
        {
            return curr.Count < ee.Path?.Length;
        }

        private Path FindBestSolution(List<Path>? sols)
        {
            if (sols == null) return null;
            return sols.OrderBy(x => x.NodeDepth).First();
        }

        private SolutionDTO CreateDTO(SolverState solverState, Path solution)
        {
            ISolver solver = solverState.Solver;
            return new SolutionDTO
            {
                IsAutomated        = true,
                PuzzleIdent        = solverState.Command.PuzzleIdent?.ToString(),
                Path               = solution.ToString(),
                Created            = DateTime.Now,
                Modified           = DateTime.Now,
                MachineName        = Environment.MachineName,
                MachineCPU         = DevHelper.DescribeCPU(),
                SolverType         = solver.GetType().Name,
                SolverVersionMajor = solver.VersionMajor,
                SolverVersionMinor = solver.VersionMinor,
                SolverDescription  = solver.VersionDescription,
                TotalNodes         = solverState.GlobalStats.TotalNodes,
                TotalSecs          = solverState.GlobalStats.DurationInSec,
                Description        = solution.Description
            };
        }

        private IReadOnlyCollection<SolutionDTO>? GetSolutionsForMachineAndSolver(PuzzleIdent puzzleIdent, ISolver solver)
        {

            var existingSolutions = repo.GetPuzzleSolutions(puzzleIdent);
            if (existingSolutions is null) return null;

            return existingSolutions.Where(x => x.MachineName == Environment.MachineName && x.SolverType == solver.GetType().Name).ToArray();

        }

        //  private int StoreAttempt(ISolver solver, LibraryPuzzle dto, SolverState state, string desc, out string res)
        // {
        //     var best = state.Solutions?.OrderBy(x => x.Count).FirstOrDefault();
        //
        //
        //     var exists = repo.GetPuzzleSolutions(dto.Ident);
        //     if (exists != null && exists.Any())
        //     {
        //         var onePerMachine= exists.FirstOrDefault(x => x.MachineName == attempt.MachineName && x.SolverType == attempt.SolverType);
        //         if (onePerMachine != null)
        //         {
        //             if (attempt.HasSolution )
        //             {
        //                 if (!onePerMachine.HasSolution)
        //                 {
        //                     attempt.SolutionId = onePerMachine.SolutionId; // replace
        //                     repo.Store(attempt);
        //                     res = $"Replacing Existing Solution";
        //                     return attempt.SolutionId;
        //                 }
        //                 else if (attempt.TotalNodes < onePerMachine.TotalSecs)
        //                 {
        //                     attempt.SolutionId = onePerMachine.SolutionId; // replace
        //                     repo.Store(attempt);
        //                     res = $"Replacing Existing Solution";
        //                     return attempt.SolutionId;
        //                 }
        //                 else
        //                 {
        //                     res = $"Dropping Attempt";
        //                 }
        //
        //             }
        //             else
        //             {
        //                 if (!onePerMachine.HasSolution && attempt.TotalNodes > onePerMachine.TotalNodes)
        //                 {
        //                     attempt.SolutionId = onePerMachine.SolutionId; // replace
        //                     repo.Store(attempt);
        //                     res = $"Replacing Existing Solution";
        //                     return attempt.SolutionId;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             repo.Store(attempt);
        //             res = $"Storing Attempt";
        //             return attempt.SolutionId;
        //         }
        //     }
        //     else
        //     {
        //         repo.Store(attempt);
        //         res = $"Storing Attempt";
        //         return attempt.SolutionId;
        //     }
        //
        //     res = $"Discarded";
        //     return -1;
        // }
    }
}
