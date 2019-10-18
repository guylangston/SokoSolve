using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using VectorInt;

namespace SokoSolve.Console
{
    public class SimpleConsoleGameClient : SokobanGameLogic
    {
        private Dictionary<char, CHAR_INFO_Attr> theme;
        private Dictionary<char, char> themeChar;
        private IBufferedAbsConsole<CHAR_INFO> console;
        private IRenderer<CHAR_INFO> renderer;
        Stopwatch timer = new Stopwatch();
        
        public bool EnableMouse { get; set; }

        public void Init()
        {
            System.Console.CursorSize = 1;
            System.Console.CursorVisible = false;
            
            DirectConsole.Setup(80, 25, 7*2, 14*2, "Courier New");
            DirectConsole.MaximizeWindow();
            DirectConsole.Fill(' ', 0);

            if (EnableMouse)
            {
                DirectConsole.EnableMouseSupport();
            }

            this.console = DirectConsole.Singleton;
            this.renderer = new ConsoleRendererCHAR_INFO(console);
            
            Current = Puzzle.Builder.DefaultTestPuzzle();
            
            // TODO: Mouse Interaction https://stackoverflow.com/questions/1944481/console-app-mouse-click-x-y-coordinate-detection-comparison
            
            theme = new Dictionary<char, CHAR_INFO_Attr>()
            {
                {Current.Definition.Void.Underlying,  CHAR_INFO_Attr.BACKGROUND_GRAY },
                {Current.Definition.Wall.Underlying,  CHAR_INFO_Attr.BACKGROUND_GRAY},
                {Current.Definition.Floor.Underlying, CHAR_INFO_Attr.FOREGROUND_GRAY },
                {Current.Definition.Goal.Underlying,  CHAR_INFO_Attr.FOREGROUND_GRAY },
                {Current.Definition.Crate.Underlying, CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.BACKGROUND_BLUE },
                {Current.Definition.CrateGoal.Underlying,  CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {Current.Definition.Player.Underlying,   CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {Current.Definition.PlayerGoal.Underlying,   CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY },
            };
            themeChar = Current.Definition.ToDictionary(x => x.Underlying, x => x.Underlying);
            
            // https://www.fileformat.info/info/unicode/block/box_drawing/list.htm
            // http://www.fileformat.info/info/unicode/block/block_elements/images.htm
            //themeChar[Current.Definition.Wall.Underlying] = '░';
            //themeChar[Current.Definition.Void.Underlying] = '▓';
            themeChar[Current.Definition.Void.Underlying] = ' ';
            themeChar[Current.Definition.Floor.Underlying] = ' ';
            themeChar[Current.Definition.PlayerGoal.Underlying] = 'P';
            themeChar[Current.Definition.CrateGoal.Underlying] = '@';
            
            timer.Start();
        }

        protected virtual bool HandleInput(out bool exitRequested)
        {
            exitRequested = false;

            if (EnableMouse)
            {
                this.MousePosition = DirectConsole.GetMousePosition();
            }
            if (!System.Console.KeyAvailable) return false;
            
            var k = System.Console.ReadKey();

            switch (k.Key)
            {
                case ConsoleKey.Escape:
                    exitRequested = true;
                    break;
                    
                case ConsoleKey.UpArrow:
                    base.Move(VectorInt2.Up);
                    break;
                case ConsoleKey.DownArrow:
                    base.Move(VectorInt2.Down);
                    break;
                case ConsoleKey.LeftArrow:
                    base.Move(VectorInt2.Left);
                    break;
                case ConsoleKey.RightArrow:
                    base.Move(VectorInt2.Right);
                    break;
                    
                case ConsoleKey.R:
                    base.Reset();
                    break;
                    
                case ConsoleKey.U:
                case ConsoleKey.Backspace:
                    base.UndoMove();
                    break;
            }

            return true;

        }

        public VectorInt2 MousePosition { get; set; }  = new VectorInt2(-1);

        public bool Step()
        {
            Draw();

            System.Console.CursorLeft = 0;
            System.Console.CursorTop = 0;

            if (!HandleInput(out var exitRequested))
            {
                Thread.Sleep(50);
            }
            
            return exitRequested;
        }


        private void Draw()
        {
            var puzzle = new RectInt(0, 0, Current.Width, Current.Height);
            var pos = RectInt.CenterAt(renderer.Geometry.C, puzzle);
            foreach (var tile in Current)
            {
                renderer[pos.TL + tile.Position] = new CHAR_INFO(themeChar[ tile.Value.Underlying], theme[tile.Value.Underlying]);
            }
            
            renderer.Box(pos.Outset(2,2,2,2), new CHAR_INFO('+') );

            var txtStyle= new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY);
            var txt = timer.Elapsed.ToString();
            var txtPos = renderer.Geometry.TM - new VectorInt2(txt.Length /2, 0);
            renderer.DrawText(txtPos.X, txtPos.Y, txt, txtStyle );

            if (MousePosition.X > 0)
            {
                renderer.DrawText(0,0, MousePosition.ToString().PadRight(20), txtStyle);

                if (pos.Contains(MousePosition))
                {
                    var pz = MousePosition - pos.TL;
                    var pc = Current[pz];
                    renderer.DrawText(0,1, $"{pz} -> {pc.Underlying}".PadRight(40), txtStyle);
                }
                else
                {
                    renderer.DrawText(0,1, $"".PadRight(40), txtStyle);
                }
            }
            
            renderer.Update();
        }
    }
}