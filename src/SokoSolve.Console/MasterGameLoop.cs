using System;
using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using VectorInt;

namespace SokoSolve.Console
{
    public class MasterGameLoop : GameLoopBase
    {
        private GameLoopProxy puzzle;
        private IBufferedAbsConsole<CHAR_INFO> console;
        private ConsoleRendererCHAR_INFO renderer; 
      

        public MasterGameLoop()
        {
            
        }
        public override void Init()
        {
            System.Console.CursorVisible = false;
            System.Console.OutputEncoding = Encoding.Unicode;
            
            DirectConsole.Setup(80, 25, 7*2, 10*2, "Consolas");
            DirectConsole.Fill(' ', 0);
            DirectConsole.MaximizeWindow();

            this.console = DirectConsole.Singleton;
            this.renderer = new ConsoleRendererCHAR_INFO(console);
            
            puzzle = new PuzzleGameLoop(this, renderer);
            puzzle.Init();

        }

        public override void Step(float elapsedSec)
        {
            puzzle.Step(elapsedSec);
        }

        public override void Draw()
        {
            puzzle.Draw();
        }

        public override void Dispose()
        {
            puzzle.Dispose();
        }
    }
}