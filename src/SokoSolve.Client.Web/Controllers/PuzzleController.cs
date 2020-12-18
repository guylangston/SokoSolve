using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SokoSolve.Client.Web.Logic;
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
        private readonly LibraryComponent compLib;
        private readonly ISokobanSolutionRepository repSol;
        private readonly ServerSideStateComponent compState;
        private readonly GraphVisWrapper wrapGraphVis;

        public PuzzleController(LibraryComponent compLib, ISokobanSolutionRepository repSol, ServerSideStateComponent compState, GraphVisWrapper wrapGraphVis)
        {
            this.compLib      = compLib;
            this.repSol       = repSol;
            this.compState    = compState;
            this.wrapGraphVis = wrapGraphVis;
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
            int duration = 1, 
            int totalNodes = int.MaxValue,
            string lookup = SolverBuilder.LookupFactoryDefault, 
            string solver = SolverBuilder.SolverFactoryDefault)
        {
            var ident = PuzzleIdent.Parse(id);
            var p= compLib.GetPuzzleWithCaching(ident);
            
            var report = new StringBuilder();
            var progress = new StringBuilder();
            
            var model = new SolverModel()
            {
                Puzzle = p,
                Report = report,
                Progress = progress
            };

            var builder = new SolverBuilder(this.compLib)
            {
                // Capture state creation and store on model
                GlobalEnrichState = (s) => {
                    model.State   = s;
                    model.Command = s.Command;
                }
            };
            var solverArgs = new Dictionary<string, string>()
            {
                {"solver", solver},
                {"pool", lookup},
                {"min", duration.ToString()},
                {"stopOnSolution", stopOnSolution.ToString()},
                {"maxNodes", totalNodes.ToString()}
            };

            var runner = new SingleSolverBatchSolveComponent(
                new TextWriterAdapter(new StringWriter(report)),
                new TextWriterAdapter(new StringWriter(report)),
                null, null,
                5,
                false);

            var run = new SolverRun()
            {
                p
            };

            var lease = compState.CreateLease(model);
            model.Token = lease.LeaseId;

            model.Task = Task.Run(() =>
            {
                runner.SolveOneSolverManyPuzzles(run, true, builder, solverArgs);
                
                model.RootForward = model.State.GetRootForward();
                model.RootReverse = model.State.GetRootReverse();
                model.State.Solver.Solve(model.State);
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
            
            var solverCommand = new SolverCommand(
                p, 
                new ExitConditions()
                {
                    Duration       = TimeSpan.FromMinutes(0),
                    StopOnSolution = true
                }, 
                SolverContainerByType.DefaultEmpty);
            var model = new SolverModel()
            {
                Puzzle  = p,
                Command = solverCommand,
            };

            var lease = compState.CreateLease(model);
            model.Token = lease.LeaseId;

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
            public int           Token       { get; set; }
            public LibraryPuzzle Puzzle      { get; set; }
            public Task          Task        { get; set; }
            public SolverCommand Command     { get; set; }
            public SolverState   State       { get; set; }
            public SolverNode?   RootForward { get; set; }
            public SolverNode?   RootReverse { get; set; }
            public bool          IsFinished  { get; set; }
            public bool          IsRunning   => !IsFinished;
            
            
            public StringBuilder Report   { get; set; }
            public StringBuilder Progress { get; set; }
        }
        
        public IActionResult SolveMem(string id, int token) 
            => View(compState.GetLeaseData<SolverModel>(token));

        public IActionResult NodeList(string id, int token, int? depth)
        {
            var state = compState.GetLeaseData<SolverModel>(token);
            return View(state.RootForward.Recurse().Where(x=>x.GetDepth() == depth));
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
        
        public IActionResult SolveNode(string id, int token, int? nodeid)
        {
            var state = compState.GetLeaseData<SolverModel>(token);
            
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

        public async Task<IActionResult> PathToRoot(string id, int token, int? nodeid, bool raw)
        {
            var state = compState.GetLeaseData<SolverModel>(token);
            
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
            
            return await wrapGraphVis.RenderToActionResult(sb.ToString());
        }


        public IActionResult RunningSolverReport(string id, int token)
        {
            var state = compState.GetLeaseData<SolverModel>(token);
            
            var sb = state.Report;
            return new ContentResult()
            {
                ContentType = "text/plain",
                Content     = sb?.ToString() ?? "NO_CONTENT"
            };
        }

        public IActionResult ByDepth(string id, int token)
        {
            var state = compState.GetLeaseData<SolverModel>(token);
            return View("ByDepth", state);
        }
        
        public IActionResult Workers(string id, int token)
        {
            var state = compState.GetLeaseData<SolverModel>(token);
            if (state.State is MultiThreadedSolverState multi)
            {
                return View(multi);
            }
            else
            {
                throw new NotSupportedException();
            }
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