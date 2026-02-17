using System.Text;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Game;
using SokoSolve.Game.Scenes;

namespace SokoSolve.Console
{
    internal static class PlayCommand
    {
        public static void Run()
        {
#if WINDOWS

                System.Console.CursorVisible = false;
                System.Console.OutputEncoding = Encoding.Unicode;

                var scale = 1.5;
                var charScale = 3;

                // Setup: Display
                //DirectConsole.MaximizeWindow();
                var cons = DirectConsole.Setup(
                    (int) (80 * scale),
                    (int) (25 * scale),
                    7 * charScale,
                    10 * charScale,
                    "Consolas");

                var renderer = new ConsoleRendererCHAR_INFO(cons);
                var bridge = new BridgeSokobanPixelToCHAR_INFO(renderer);

                // Setup: Input
                var input = new InputProvider()
                {
                    IsMouseEnabled = true
                };

                using (var consoleLoop = new ConsoleGameLoop<SokobanPixel>(input, bridge))
                {
                    using (var master = new SokoSolveMasterGameLoop(consoleLoop))
                    {
                        consoleLoop.Scene = master;
                        consoleLoop.Init();
                        consoleLoop.Start();
                    }
                }
#else
                //System.Console.SetBufferSize(120, 60);
                //System.Console.SetWindowSize(120, 60);
                var bridge = new BridgeSokobanPixelToConsolePixel(new ConsolePixelRenderer(BasicDirectConsole.Singleton));

                // Setup: Input
                var input = new InputProvider()
                {
                    IsMouseEnabled = false
                };

                using (var consoleLoop = new ConsoleGameLoop<SokobanPixel>(input, bridge))
                {
                    consoleLoop.SetGoalFramesPerSecond(5);
                    using (var master = new SokoSolveMasterGameLoop(consoleLoop))
                    {
                        consoleLoop.Scene = master;
                        consoleLoop.Init();
                        consoleLoop.Start();
                    }
                }
#endif

        }
    }
}
