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
using SokoSolve.Core.Components;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver;
using SokoSolve.Drawing;
using SokoSolve.Drawing.GraphVis;
using ExitConditions = SokoSolve.Core.Solver.ExitConditions;
using Path = System.IO.Path;

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
        
        public IActionResult SolveStart(
            string id, 
            bool stopOnSolution = true, 
            double duration = 1, 
            string lookup = BatchSolveComponent.LookupFactoryDefault, 
            string solver = BatchSolveComponent.SolverFactoryDefault)
        {
            var ident = PuzzleIdent.Parse(id);
            var p= compLib.GetPuzzleWithCaching(ident);
            
            var solverObj      =  BatchSolveComponent.SolverFactory.GetInstance(solver);
            var report      = new StringBuilder();
            var solverCommand = new SolverCommand(p.Puzzle, new ExitConditions()
            {
                Duration       = TimeSpan.FromMinutes(duration),
                StopOnSolution = stopOnSolution
            })
            {
                ServiceProvider = new SolverContainerByType(new Dictionary<Type, Func<Type, object>>()
                {
                    { typeof(StringBuilder),              _ => report },
                    { typeof(INodeLookup),                _ => BatchSolveComponent.LookupFactory.GetInstance(lookup)},
                    { typeof(ISolverQueue),               _ => new SolverQueueConcurrent()},
                    // { typeof(ISolverRunTracking),         _ => runTracking},
                    // { typeof(ISokobanSolutionRepository), _ => solutionRepo},
                }),
                Report = new TextWriterAdapter(new StringWriter(report))
            };
            var model = new SolverModel()
            {
                Token = DateTime.Now.Ticks,
                Puzzle = p,
                Command = solverCommand,
                State = solverObj.Init(solverCommand)
            };

            staticState[model.Token] = model;

            model.Task = Task.Run(() =>
            {
                model.RootForward = model.State.GetRootForward();
                model.RootReverse = model.State.GetRootReverse();
                solverObj.Solve(model.State);
                
                model.IsFinished = true;
            });

            return RedirectToAction("SolveMem", new {id, token=model.Token});
        }
        
        public IActionResult Saved()
        {
            var model = new List<string>();
            model.AddRange(Directory.GetFiles(@"C:\Projects\SokoSolve\data\SavedState"));
            model.AddRange(Directory.GetFiles(@"E:\temp\SokoSolve"));
            return View(model);
        }
        
        public IActionResult StartFromFile(string file)
        {
            var fileName = Path.GetFileName(file);
            var ident = PuzzleIdent.Parse(fileName.Substring(0, fileName.IndexOf("-")));
            var p     = compLib.GetPuzzleWithCaching(ident);
            
            var solverCommand = new SolverCommand(p.Puzzle, new ExitConditions()
                {
                    Duration       = TimeSpan.FromMinutes(0),
                    StopOnSolution = true
                });
            var model = new SolverModel()
            {
                Token   = DateTime.Now.Ticks,
                Puzzle  = p,
                Command = solverCommand,
            };

            staticState[model.Token] = model;

            model.Task = Task.Run(() =>
            {
                var ser = new BinaryNodeSerializer();
                using (var f = System.IO.File.OpenRead(file))
                {
                    using (var br = new BinaryReader(f))
                    {
                        model.RootForward = ser.AssembleTree(br);
                    }
                    
                }
                model.IsFinished = true;
            });

            return RedirectToAction("SolveMem", new {id=ident.ToString(), token =model.Token});
        }

        public class SolverModel
        {
            public long                Token      { get; set; }
            public LibraryPuzzle       Puzzle     { get; set; }
            public Task                Task       { get; set; }
            public SolverCommand       Command    { get; set; }
            public SolverState State     { get; set; }
            public SolverNode? RootForward { get; set; }
            public SolverNode? RootReverse { get; set; }
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
        
        public IActionResult NodeList(string id, long token, int? depth)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                return View(state.RootForward.Recurse().Where(x=>x.GetDepth() == depth));    
            }

            return RedirectToAction("Home", new {id});
        }

        public class NodeModel
        {
            public SolverModel Solver { get; set; }
            public int? NodeId { get; set; }
            public SolverNode Node { get; set; }
            public long Token { get; set; }
            public INodeLookup? PoolFwd { get; set; }
            public INodeLookup? PoolRev { get; set; }
        }
        
        public IActionResult SolveNode(string id, long token, int? nodeid)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                var node = GetNode(state, nodeid);

                var multiResult = state.State as MultiThreadedSolverState;

                return View(new NodeModel()
                {
                    Token   = token,
                    Solver  = state,
                    Node    = node,
                    NodeId  = nodeid,
                    PoolFwd = multiResult?.PoolForward,
                    PoolRev = multiResult?.PoolReverse
                });
            }

            return RedirectToAction("Home", new {id, txt="NotFound"});
        }

        private static SolverNode GetNode(SolverModel state, int? nodeid)
        {
            var node = nodeid == null
                ? state.RootForward
                : nodeid > 0
                    ? (state.RootForward?.Recurse().FirstOrDefault(x => x.SolverNodeId == nodeid.Value)
                       ?? state.RootReverse?.Recurse().FirstOrDefault(x => x.SolverNodeId == nodeid.Value))
                    : state.RootReverse;
            return node;
        }

        public IActionResult PathToRoot(string id, long token, int? nodeid, bool raw)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                var node = GetNode(state, nodeid);

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
                    
                var render = new GraphVisRender();
                var sb     = new StringBuilder();
                using (var ss = new StringWriter(sb))
                {
                    render.Render(expanded, ss);
                }
                

                var getStartProcessQuery = new GetStartProcessQuery();
                var getProcessStartInfoQuery = new GetProcessStartInfoQuery();
                var registerLayoutPluginCommand = new RegisterLayoutPluginCommand(getProcessStartInfoQuery, getStartProcessQuery);
                var wrapper = new GraphGeneration(getStartProcessQuery, getProcessStartInfoQuery, registerLayoutPluginCommand);
                var b = wrapper.GenerateGraph(sb.ToString(), Enums.GraphReturnType.Svg);

                if (raw)
                {
                        
                }

                return new FileContentResult(b, "image/svg+xml"); 
            }

            return RedirectToAction("Home", new {id, txt ="NotFound"});
        }


        public IActionResult RunningSolverReport(string id, long token)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                var sb = state.Command.ServiceProvider.GetInstance<StringBuilder>();
                return new ContentResult()
                {
                    ContentType = "text/plain",
                    Content = sb.ToString()
                };
            }

            return RedirectToAction("Home", new {id, txt ="NotFound"});
        }

        public IActionResult ByDepth(string id, long token)
        {
            if (staticState.TryGetValue(token, out var state))
            {
                return View("ByDepth", state);
            }

            return RedirectToAction("Home", new {id, txt ="NotFound"});
        }
        
        public IActionResult Workers(string id, long token)
        {
            if (staticState.TryGetValue(token, out var model))
            {
                if (model.State is MultiThreadedSolverState multi)
                {
                    return View(multi);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return RedirectToAction("Home", new {id, txt ="NotFound"});
        }
        public IActionResult Solution(string puzzle, int sid)
        {
            var ident = PuzzleIdent.Parse(puzzle);
            var p     = compLib.GetPuzzleWithCaching(ident);
            var sols  = repSol.GetPuzzleSolutions(ident);
            return View((p, sols.First(x=>x.SolutionId == sid)));
        }
    }
}