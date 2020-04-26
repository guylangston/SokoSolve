using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using GraphVizWrapper;
using GraphVizWrapper.Commands;
using GraphVizWrapper.Queries;
using Microsoft.AspNetCore.Mvc;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using SokoSolve.Drawing;
using TextRenderZ;
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
        
        public IActionResult Svg(string id, float w=16, float h=16)
        {
            var ident = PuzzleIdent.Parse(id);
            var p     = compLib.GetPuzzleWithCaching(ident);
            var sols  = repSol.GetPuzzleSolutions(ident);
            
            var sb = new StringBuilder();

            using (var tw = new StringWriter(sb))
            {
                var dia = new PuzzleDiagram()
                {
                    GetResource = x => "/img/"+x
                };
                dia.Draw(tw, p.Puzzle, new Vector2(w,h));    
            }

            return Content(sb.ToString(),"image/svg+xml");
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
        
        public IActionResult SolveStart(string id, double duration = 1, bool stopOnSolution =true)
        {
            var ident = PuzzleIdent.Parse(id);
            var p= compLib.GetPuzzleWithCaching(ident);
            
            var solver = new MultiThreadedForwardReverseSolver(new SolverNodeFactoryTrivial());
            var solverCommand = new SolverCommand()
            {
                Puzzle = p.Puzzle,
                ExitConditions = new ExitConditions()
                {
                    Duration = TimeSpan.FromMinutes(duration),
                    StopOnSolution = false
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

        public class NodeModel
        {
            public SolverModel Solver { get; set; }
            public int? NodeId { get; set; }
            public SolverNode Node { get; set; }
            public long Token { get; set; }
            public ISolverPool? PoolFwd { get; set; }
            public ISolverPool? PoolRev { get; set; }
        }
        
        public IActionResult SolveNode(string id, long token, int? nodeid)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                if (state.Result is MultiThreadedSolverBaseResult multiResult)
                {
                    var node = nodeid == null 
                        ? multiResult.Root
                        : nodeid > 0
                            ? multiResult.Root.FirstOrDefault(x=>x.SolverNodeId == nodeid.Value) ?? multiResult.RootReverse.FirstOrDefault(x=>x.SolverNodeId == nodeid.Value)
                            : multiResult.RootReverse;
                    
                    

                    return View(new NodeModel()
                    {
                        Token = token,
                        Solver = state,
                        Node   = node,
                        NodeId = nodeid,
                        PoolFwd = multiResult.PoolForward,
                        PoolRev = multiResult.PoolReverse
                    });
                } 
            }

            return RedirectToAction("Home", new {id, txt="NotFound"});
        }
        
        public IActionResult PathToRoot(string id, long token, int nodeid, bool raw)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                if (state.Result is MultiThreadedSolverBaseResult multiResult)
                {
                    var node = nodeid == null 
                        ? multiResult.Root
                        : nodeid > 0
                            ? multiResult.Root.FirstOrDefault(x=>x.SolverNodeId == nodeid) ?? multiResult.RootReverse.FirstOrDefault(x=>x.SolverNodeId == nodeid)
                            : multiResult.RootReverse;

                    var path = node.PathToRoot();

                    var expanded = new List<SolverNode>();
                    foreach (var p in path)
                    {
                        expanded.Add(p);
                        if (p.HasChildren)
                        {
                            foreach (var kid in p.Children)
                            {
                                if (!path.Contains(kid)) expanded.Add(kid);
                            }
                        }
                    }

                    var sb = FluentString.Create()
                         .AppendLine("digraph{ rankdir=TB;")
                         .ForEach(expanded, (fb, x) =>
                         {
                             var shape = "circle";
                             if (x.Parent == null) shape = "doublecircle";
                             if (x.Status == SolverNodeStatus.Dead || x.Status == SolverNodeStatus.DeadRecursive) shape = "square";
                             if (x.Status == SolverNodeStatus.Evaluted) shape = "diamond";
                             
                             //fb.AppendLine($"{x.SolverNodeId} [label=\"{x.SolverNodeId}\" shape={shape} href=\"/Puzzle/SolveNode/{id}?token={token}&nodeid={x.SolverNodeId}\"]");
                             fb.AppendLine($"{x.SolverNodeId} [label=\"{x.SolverNodeId}\" shape=\"{shape}\" href=\"http://www.udk.com\"]");
                         })
                         .AppendLine("")
                         .ForEach(expanded.Where(x=>x.Parent != null), (fb, x) => fb.AppendLine( $"{x.SolverNodeId} -> {x.Parent?.SolverNodeId.ToString() ?? "null"}"))
                         .Append("}");

                    var getStartProcessQuery = new GetStartProcessQuery();
                    var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
                    var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
                    var wrapper = new GraphGeneration(getStartProcessQuery, getProcessStartInfoQuery, registerLayoutPluginCommand);
                    var b = wrapper.GenerateGraph(sb, Enums.GraphReturnType.Svg);

                    if (raw)
                    {
                        
                    }

                    return new FileContentResult(b, "image/svg+xml"); 
                } 
            }

            return RedirectToAction("Home", new {id, txt ="NotFound"});
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