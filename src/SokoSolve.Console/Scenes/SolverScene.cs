using System;

using System.Threading.Tasks;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using SokoSolve.Core.Solver;
using VectorInt;

namespace SokoSolve.Console.Scenes
{
    public class SolverScene : GameLoopProxy<MasterGameLoop>
    {
        private readonly ConsoleRendererCHAR_INFO renderer;
        public           Puzzle                   Puzzle        { get; }
        public           ISolver?                 Solver        { get; private set; }
        public           SolverCommand            SolverCommand { get; private set; }
        public           Task                     SolverTask    { get; private set; }
        public           SolverCommandResult      SolverState   { get; private set; }

        public SolverScene(MasterGameLoop parent, Puzzle puzzle) : base(parent)
        {
            Puzzle = puzzle;
            this.renderer = Parent.Renderer;
        }
        
        public override void Init()
        {
            
        }

        public override void Step(float elapsedSec)
        {
            if (Parent.Input.IsKeyPressed(ConsoleKey.Escape))
            {
                if (SolverTask != null)
                {
                    SolverTask.Dispose();
                    SolverTask = null;
                }
                Parent.ShowLibrary();
            }
            
            if (Solver is null)
            {
                if (Parent.Input.IsKeyPressed(ConsoleKey.F) || Parent.Input.IsKeyPressed(ConsoleKey.Enter))
                {
                    Solver = new SingleThreadedForwardSolver();
                    SolverCommand = new SolverCommand()
                    {
                        Puzzle = Puzzle,
                        ExitConditions = ExitConditions.Default3Min,
                    };

                    SolverTask = Task.Run(() =>
                    {
                        SolverState = Solver.Init(SolverCommand);
                        Solver.Solve(SolverState);
                    });
                }
                if (Parent.Input.IsKeyPressed(ConsoleKey.R))
                {
                    Solver = new SingleThreadedReverseSolver();
                    SolverCommand = new SolverCommand()
                    {
                        Puzzle         = Puzzle,
                        ExitConditions = ExitConditions.Default3Min,
                    };

                    SolverTask = Task.Run(() =>
                    {
                        SolverState = Solver.Init(SolverCommand);
                        Solver.Solve(SolverState);
                    });
                }
                else  if (Parent.Input.IsKeyPressed(ConsoleKey.M) )
                {
                    Solver = new MultiThreadedForwardReverseSolver();
                    SolverCommand = new SolverCommand()
                    {
                        Puzzle         = Puzzle,
                        ExitConditions = ExitConditions.Default3Min,
                    };

                    SolverTask = Task.Run(() =>
                    {
                        SolverState = Solver.Init(SolverCommand);
                        Solver.Solve(SolverState);
                    });
                }
            }
            else
            {
              
                
            }
        }

        public CHAR_INFO DefaultStyle { get; } = new CHAR_INFO('.', CHAR_INFO_Attr.FOREGROUND_GRAY);

        public override void Draw()
        {
            renderer.Fill(new CHAR_INFO());
            
            var rectPuzzle = Puzzle.Area.Move((2,4));
            renderer.Box(rectPuzzle.Outset(2), DrawingHelper.AsciiBox);
            renderer.DrawMap(Puzzle, rectPuzzle.TL, x=>new CHAR_INFO(x.Underlying));
            
            if (Solver is null)
            { 
                var stats = RectInt.FromTwoPoints(renderer.Geometry.TM, renderer.Geometry.BR);
                renderer.TitleBox(stats, "Select Solver", DrawingHelper.AsciiBox);
                
                var start = stats.TL + (2, 2);
                start = renderer.DrawText(start, $"[F] {nameof(SingleThreadedForwardSolver)}", DefaultStyle);
                start = renderer.DrawText(start, $"[R] {nameof(SingleThreadedReverseSolver)}", DefaultStyle);
                start = renderer.DrawText(start, $"[M] {nameof(MultiThreadedForwardReverseSolver)}", DefaultStyle);
            }
            else
            {
                // Solver Running, show progress
                if (SolverTask.IsCompleted)
                {
                    // Done
                    renderer.DrawText((0,0), $"[DONE] Solutions Found = {SolverState.Solutions.Count}", DefaultStyle);
                }
                else if (SolverTask.IsFaulted)
                {
                    // Error
                    renderer.DrawText((0,0), SolverTask.Exception.Message, DefaultStyle);
                }
                else
                {
                    // Running
                    renderer.DrawText((0,0), $"[RUNNING] {SolverState?.Statistics?.Elapased}", DefaultStyle);
                }
                
                // For all
                if (SolverState?.Statistics != null)
                {
                    var s = SolverState.Statistics;
                    var stats = RectInt.FromTwoPoints(renderer.Geometry.TM, renderer.Geometry.BR);
                    renderer.TitleBox(stats, "Statistics", DrawingHelper.AsciiBox);

                    var start = stats.TL + (2, 2);
                    start = renderer.DrawText(start, $"Solutions: {SolverState.Solutions?.Count}", DefaultStyle);
                    start = renderer.DrawText(start, $"Nodes: {s.TotalNodes}", DefaultStyle);
                    start = renderer.DrawText(start, $"Nodes: {s.TotalNodes / s.DurationInSec:0.0}/sec", DefaultStyle);

                    if (Solver.Statistics != null)
                    {
                        foreach (var stLine in Solver.Statistics)
                        {
                            start = renderer.DrawText(start, stLine.ToStringShort(), DefaultStyle);        
                        }   
                    }
                }
            }
            
            Parent.Renderer.Update();
        }

        public override void Dispose()
        {
            
        }
    }
}