using System;
using System.Threading.Tasks;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Solver;
using SokoSolve.Core.Solver.Solvers;
using VectorInt;

namespace SokoSolve.Game.Scenes
{
    public class SolverScene : GameScene<SokoSolveMasterGameLoop, SokobanPixel>
    {
        private Puzzle         Puzzle          { get; }
        private ISolver?       Solver          { get; set; }
        private SolverCommand? SolverCommand   { get; set; }
        private Task?          SolverTask      { get; set; }
        private SolverState?   SolverState     { get; set; }
        private DisplayStyle   Style           => Parent.Style;
        public  Exception?     SolverException { get; set; }
        
        public SolverScene(SokoSolveMasterGameLoop parent, Puzzle puzzle) : base(parent)
        {
            Puzzle = puzzle;
        }

        public override void Init()
        {
            
        }

        public override void Step(float elapsedSec)
        {
            if (Input.IsKeyPressed(ConsoleKey.Escape))
            {
                if (Solver != null)
                {
                    if (SolverCommand != null) SolverCommand.ExitConditions.ExitRequested = true;

                    if (SolverTask != null)
                    {
                        SolverTask.Wait();
                        SolverTask.Dispose();
                        SolverTask = null;
                        SolverCommand = null;
                        SolverState = null;
                        Solver = null;
                    }
                }
                
                Parent.ShowLibrary();
            }
            if (Input.IsKeyPressed(ConsoleKey.Backspace))
            {
                if (Solver != null)
                {
                    if (SolverCommand != null)
                    {
                        SolverCommand.ExitConditions.ExitRequested = true;

                        if (SolverTask != null)
                        {
                            SolverTask.Wait();
                            SolverTask.Dispose();
                            SolverTask = null;
                            SolverCommand = null;
                            SolverState = null;
                            Solver = null;
                        }
                    }
                }
            }            
            if (Solver is null)
            {
                if (Input.IsKeyPressed(ConsoleKey.F) || Input.IsKeyPressed(ConsoleKey.Enter))
                {
                    throw new NotImplementedException();
                    //RunSolverWith(new SingleThreadedForwardSolver(SolverCommand, new SolverNodePoolingFactoryDefault()));
                }
                if (Input.IsKeyPressed(ConsoleKey.R))
                {
                    throw new NotImplementedException();
                    //RunSolverWith(new SingleThreadedReverseSolver(SolverCommand, new SolverNodePoolingFactoryDefault()));
                }
                else if (Input.IsKeyPressed(ConsoleKey.M) )
                {
                    throw new NotImplementedException();
                    //RunSolverWith(new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault()));
                }
            }
            else
            {
                

            }
        }

        private void RunSolverWith(ISolver solver)
        {
            SolverException = null;
            SolverState = null;

            Solver = solver;
            SolverCommand = new SolverCommand(Puzzle, PuzzleIdent.Temp(), ExitConditions.Default3Min(), SolverContainerByType.DefaultEmpty);

            SolverTask = Task.Run(() =>
            {
                try
                {
                    SolverState = Solver.Init(SolverCommand);
                    Solver.Solve(SolverState);
                }
                catch (Exception e)
                {
                    SolverException = e;
                }
                
            });
        }

        

        public override void Draw()
        {
            var rectPuzzle = Puzzle.Area.Move((2,4));
            Renderer.Box(rectPuzzle.Outset(2));
            Renderer.DrawMap(Puzzle, rectPuzzle.TL, x=>Style[x]);
            
            if (Solver is null)
            { 
                var stats = RectInt.FromTwoPoints(Renderer.Geometry.TM, Renderer.Geometry.BR);
                Renderer.TitleBox(stats, "Select Solver");
                
                var start = stats.TL + (2, 2);
                start = Renderer.DrawText(start, $"[F] {nameof(SingleThreadedForwardSolver)}", Style.DefaultPixel);
                start = Renderer.DrawText(start, $"[R] {nameof(SingleThreadedReverseSolver)}", Style.DefaultPixel);
                start = Renderer.DrawText(start, $"[M] {nameof(MultiThreadedForwardReverseSolver)}", Style.DefaultPixel);
            }
            else
            {
                // Solver Running, show progress
                if (SolverTask.IsFaulted || SolverException != null)
                {
                    // Error
                    Renderer.DrawText((0, 0), (SolverException ?? SolverTask.Exception).Message, Style.Error.AsPixel());
                }
                else if (SolverTask.IsCompleted)
                {
                    // Done
                    Renderer.DrawText((0,0), $"[{SolverState.Exit}:{(SolverState.EarlyExit ? "EARLY-EXIT": "")}] Solutions:{SolverState.SolutionsNodes?.Count ?? 0}", Style.DefaultPixel);
                }
                
                else
                {
                    // Running
                    Renderer.DrawText((0,0), $"[RUNNING] {SolverState?.GlobalStats?.Elapsed}", Style.DefaultPixel);
                }
                
                // For all
                if (SolverState?.GlobalStats != null)
                {
                    var s = SolverState.GlobalStats;
                    var stats = RectInt.FromTwoPoints(Renderer.Geometry.TM, Renderer.Geometry.BR);
                    Renderer.TitleBox(stats, "Statistics");

                    var start = stats.TL + (2, 2);
                    start = Renderer.DrawText(start, $"Solutions: {SolverState.SolutionsNodes?.Count}", Style.DefaultPixel);
                    start = Renderer.DrawText(start, $"Nodes: {s.TotalNodes} @  {s.TotalNodes / s.DurationInSec:0.0}/sec", Style.DefaultPixel);
                    
                    if (SolverState.Statistics != null)
                    {
                        foreach (var stLine in SolverState.Statistics)
                        {
                            start = Renderer.DrawText(start, stLine.ToStringShort(), Style.DefaultPixel);        
                        }   
                    }
                    
//                    if (SolverState is ISolverVisualisation vs && vs.TrySample(out var node))
//                    {
//                        Renderer.DrawMapWithPosition(node.CrateMap, rectPuzzle.TL, 
//                            (p, c) => new CHAR_INFO(Puzzle[p].Underlying, 
//                                c ? CHAR_INFO_Attr.BACKGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_GRAY: CHAR_INFO_Attr.FOREGROUND_GRAY));
//                        
//                    }
                }
            }
        }

        

        public override void Dispose()
        {
            
        }
    }
}