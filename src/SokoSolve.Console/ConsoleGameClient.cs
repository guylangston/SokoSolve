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


        public void Init()
        {
            System.Console.CursorSize = 1;
            System.Console.CursorVisible = false;
            
            DirectConsole.Setup(80, 25, 7*2, 14*2, "Courier New");
            DirectConsole.Fill(' ', 0);
            
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
        
        public bool Step()
        {
            Draw();

            System.Console.CursorLeft = 0;
            System.Console.CursorTop = 24;


            if (System.Console.KeyAvailable)
            {
                var k = System.Console.ReadKey();

                if (k.Key == ConsoleKey.Escape) return false;
                if (k.Key == ConsoleKey.UpArrow) base.Move(VectorInt2.Up);
                if (k.Key == ConsoleKey.DownArrow) base.Move(VectorInt2.Down);
                if (k.Key == ConsoleKey.LeftArrow) base.Move(VectorInt2.Left);
                if (k.Key == ConsoleKey.RightArrow) base.Move(VectorInt2.Right);
                if (k.Key == ConsoleKey.Backspace) base.UndoMove();
            }
            else
            {
                Thread.Sleep(100);
            }
            
            return true;
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
            
            renderer.Update();
        }
    }
}