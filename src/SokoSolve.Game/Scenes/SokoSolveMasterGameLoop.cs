using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core;
using SokoSolve.Core.Lib;
using VectorInt;

namespace SokoSolve.Game.Scenes
{

    public class SokoSolveMasterGameLoop : GameScene<IRenderingGameLoop<SokobanPixel>, SokobanPixel>
    {
        private PlayPuzzleScene?      puzzle;
        private LibraryScene?         library;
        private GameScene<SokoSolveMasterGameLoop, SokobanPixel> Current  { get; set; }

        public SokoSolveMasterGameLoop(IRenderingGameLoop<SokobanPixel> parent) : base(parent)
        {
        }

        public DisplayStyle Style { get; set; } = new DisplayStyle();

        public override void Init()
        {
            // Setup: Library
            var path = new PathHelper();
            var compLib = new LibraryComponent(path.GetLibraryPath());
            string libName = "Microban.ssx";

            library = new LibraryScene(this, compLib.LoadLibrary(compLib.GetPathData(libName)));
            library.Init();

            Current = library;
        }

        public override void Step(float elapsedSec)
        {
            if (Current == null) throw new Exception("Call Init() first");
            Current.Step(elapsedSec);
        }

        public override void Draw()
        {
            Renderer.Fill(Style.DefaultPixel);

            Current.Draw();

            Renderer.Update();
            //System.Console.Title = $"{Renderer.Geometry} @ {FramesPerSecond:0,0}fps";
        }

        public override void Dispose()
        {
            puzzle?.Dispose();
            library?.Dispose();

        }

        public void PlayPuzzle(LibraryPuzzle libraryPuzzle)
        {
            Current = puzzle = new PlayPuzzleScene(this, libraryPuzzle);
            puzzle.Init();
        }

        public void PuzzleComplete()
        {
            Current = library;
        }

        public void PuzzleGivingUp()
        {
            Current = library;
        }

        public void Solve(LibraryPuzzle libraryPuzzle)
        {
            //Parent.SetGoalFPS(10);    // We want it slow
            Current = new SolverScene(this, libraryPuzzle.Puzzle);
            Current.Init();
        }

        public void ShowLibrary()
        {
            //SetDefaultInterval();
            Current = library;
        }
    }
}
