using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Library;
using VectorInt;

namespace SokoSolve.Console.Scenes
{
    public class SokoSolveMasterGameLoop : GameScene<IRenderingGameLoop<CHAR_INFO>, CHAR_INFO>
    {
        private PlayPuzzleScene?      puzzle;
        private LibraryScene?         library;
        private GameScene<SokoSolveMasterGameLoop, CHAR_INFO> Current  { get; set; }
        
        

        public SokoSolveMasterGameLoop(IRenderingGameLoop<CHAR_INFO> parent) : base(parent)
        {
        }

        public override void Init()
        {
            // Setup: Library
            var path = new PathHelper();
            var compLib = new LibraryComponent(path.GetLibraryPath());
            string libName = "SokoSolve-v1\\Microban.ssx";
            //string libName = "SokoSolve-v1\\Sasquatch.ssx";
            library = new LibraryScene(this, compLib.LoadLibrary(compLib.GetPathData(libName)));
            library.Init();

            Current = library;
        }

        public override void Step(float elapsedSec)
        {
            Current.Step(elapsedSec);
        }

        public override void Draw()
        {
            Renderer.Fill(new CHAR_INFO());
            
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