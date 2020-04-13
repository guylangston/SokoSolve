using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using ExitConditions = SokoSolve.Core.Solver.ExitConditions;

namespace SokoSolve.Client.Web.Controllers
{
    public class PuzzleController : Controller
    {
        private static readonly ConcurrentDictionary<long, SolverModel> staticState = new ConcurrentDictionary<long, SolverModel>();
        private readonly LibraryComponent compLib;
        private readonly ISokobanSolutionRepository repSol;

        public PuzzleController(LibraryComponent compLib, ISokobanSolutionRepository repSol)
        {
            this.compLib = compLib;
            this.repSol = repSol;
        }

        public class HomeModel
        {
            public LibraryPuzzle Puzzle { get; set; }
            public StaticAnalysisMaps StaticAnalysis { get; set; }
            public IReadOnlyCollection<SolutionDTO>? Solutions { get; set; }
        }

        public IActionResult Home(string id)
        {
            var ident = PuzzleIdent.Parse(id);
            var p = compLib.GetPuzzleWithCaching(ident);
            var sols = repSol.GetPuzzleSolutions(ident);
            
            return View(new HomeModel()
            {
                Puzzle         = p,
                Solutions      = sols,
                StaticAnalysis = new StaticAnalysisMaps(p.Puzzle)
            });
        }


        

        public IActionResult StaticAnalysis(string id)
        {
            var ident = PuzzleIdent.Parse(id);
            var p= compLib.GetPuzzleWithCaching(ident);

            return View(new HomeModel()
            {
                Puzzle = p,
                StaticAnalysis = new StaticAnalysisMaps(p.Puzzle)
            });
        }
        
        public IActionResult SolveStart(string id, double mins = 1 )
        {
            var ident = PuzzleIdent.Parse(id);
            var p= compLib.GetPuzzleWithCaching(ident);
            
            var solver = new MultiThreadedForwardReverseSolver(new SolverNodeFactoryTrivial());
            var solverCommand = new SolverCommand()
            {
                Puzzle = p.Puzzle,
                ExitConditions = new ExitConditions()
                {
                    Duration = TimeSpan.FromMinutes(mins)
                }
            };
            var model = new SolverModel()
            {
                Token = DateTime.Now.Ticks,
                Puzzle = p,
                Command = solverCommand,
                Result = solver.Init(solverCommand)
            };

            staticState[model.Token] = model;

            model.Task = Task.Run(() =>
            {
                solver.Solve(model.Result);
                model.IsFinished = true;
            });

            return RedirectToAction("SolveMem", new {id, token=model.Token});
        }

        public class SolverModel
        {
            public long                Token      { get; set; }
            public LibraryPuzzle       Puzzle     { get; set; }
            public Task                Task       { get; set; }
            public SolverCommand       Command    { get; set; }
            public SolverResult Result     { get; set; }
            public bool                IsFinished { get; set; }
            public bool                IsRunning  => !IsFinished;
        }
        
        public IActionResult SolveMem(string id, long token)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                return View(state);    
            }

            return RedirectToAction("Home", new {id});
        }

        public IActionResult ReportClash(string id, long token)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                if (state.IsRunning) return Content("Must be complete");
        
                if (state.Result is MultiThreadedSolverBaseResult multiResult)
                {
                    throw new NotImplementedException();
                    // var all = multiResult.PoolForward.GetAll().Union(multiResult.PoolReverse.GetAll());
                    // var group = all.GroupBy(x => x.GetHashCode())
                    //                .OrderByDescending(x=>x.Count());
                    // return View(group);
                }

                return Content("Not Supported");

            }
        
            return RedirectToAction("Home", new {id});
        }
    }
}