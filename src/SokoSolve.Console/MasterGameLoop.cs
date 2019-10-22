using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Library;

namespace SokoSolve.Console
{
    public class MasterGameLoop : GameLoopBase
    {
        private IBufferedAbsConsole<CHAR_INFO> console;
        private ConsoleRendererCHAR_INFO renderer;
        
        private PuzzleGameLoop puzzle;
        private LibraryScene library;
        
        private GameLoopProxy Current { get; set; }
      
        public MasterGameLoop()
        {
            
        }
        
        public InputProvider Input { get; set; }
        
        
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
            this.renderer = new ConsoleRendererCHAR_INFO(console);
            
            this.Input = new InputProvider()
            {
                IsMouseEnabled = true
            };

            var path = new PathHelper();
            var compLib = new LibraryComponent(path.GetLibraryPath());
            string libName = "SokoSolve-v1\\Sasquatch.ssx";
            library = new LibraryScene(this, compLib.LoadLibrary(compLib.GetPathData(libName)), renderer);
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
            Current.Draw();
        }

        public override void Dispose()
        {
            puzzle?.Dispose();
            library?.Dispose();
            
            Input.Dispose();
        }

        public void PlayPuzzle(LibraryPuzzle libraryPuzzle)
        {
            Current = puzzle = new PuzzleGameLoop(this, renderer, libraryPuzzle);
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
    }
}