using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Library;
using VectorInt;

namespace SokoSolve.Console.Scenes
{
    public class MasterGameLoop : GameLoopBase
    {
        private IBufferedAbsConsole<CHAR_INFO>? console;
        
        private PlayPuzzleScene? puzzle;
        private LibraryScene? library;

        public MasterGameLoop()
        {
            
        }

        private GameLoopProxy? Current { get; set; }
        public InputProvider? Input { get; set; }
        public ConsoleRendererCHAR_INFO? Renderer { get; set; }
        
        public override void Init()
        {
            System.Console.CursorVisible = false;
            System.Console.OutputEncoding = Encoding.Unicode;

            var scale = 1.5;
            var charScale = 3;
            //DirectConsole.MaximizeWindow();
            DirectConsole.Setup((int)(80 * scale),  (int)(25 * scale), 7*charScale, 10*charScale, "Consolas");
            DirectConsole.Fill(' ', 0);
            
            this.console = DirectConsole.Singleton;
            this.Renderer = new ConsoleRendererCHAR_INFO(console);
            
            this.Input = new InputProvider()
            {
                IsMouseEnabled = true
            };

            var path = new PathHelper();
            var compLib = new LibraryComponent(path.GetLibraryPath());
            string libName = "SokoSolve-v1\\Sasquatch.ssx";
            library = new LibraryScene(this, compLib.LoadLibrary(compLib.GetPathData(libName)), Renderer);
            library.Init();

            Current = library;
        }

        public override void Step(float elapsedSec)
        {
            Current.Step(elapsedSec);
            Input.Step();
        }

        public override void Draw()
        {
            Renderer.Fill(new CHAR_INFO());
            Current.Draw();
            Renderer.Update();
            System.Console.Title = $"{Renderer.Geometry} @ {FramesPerSecond:0,0}fps";
        }

        public override void Dispose()
        {
            puzzle?.Dispose();
            library?.Dispose();
            
            Input.Dispose();
        }

        public void PlayPuzzle(LibraryPuzzle libraryPuzzle)
        {
            Current = puzzle = new PlayPuzzleScene(this, Renderer, libraryPuzzle);
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
            SetGoalFPS(10);
            Current = new SolverScene(this, libraryPuzzle.Puzzle);
            Current.Init();
        }

        public void ShowLibrary()
        {
            SetDefaultInterval();
            Current = library;
        }
    }
}