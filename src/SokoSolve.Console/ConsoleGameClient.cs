using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ConsoleZ;
using ConsoleZ.Drawing;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using VectorInt2 = SokoSolve.Core.Primitives.VectorInt2;

namespace SokoSolve.Console
{
    public class SimpleConsoleGameClient : SokobanGameLogic
    {
        
        private Dictionary<char, CHAR_INFO_Attr> theme;
        private Dictionary<char, char> themeChar;
        private IBufferedAbsConsole<CHAR_INFO> console;
        private IRenderer<CHAR_INFO> renderer;


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
                {Current.Definition.Void.Underlying,  CHAR_INFO_Attr.BACKGROUND_BLUE },
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
            themeChar[Current.Definition.Wall.Underlying] = '░';
            themeChar[Current.Definition.Void.Underlying] = '▓';
        }
        
        public bool Step()
        {
            Draw();

            System.Console.CursorLeft = 0;
            System.Console.CursorTop = 24;

            var k = System.Console.ReadKey();
            

            if (k.Key == ConsoleKey.Escape) return false;
            if (k.Key == ConsoleKey.UpArrow) base.Move(VectorInt2.Up);
            if (k.Key == ConsoleKey.DownArrow) base.Move(VectorInt2.Down);
            if (k.Key == ConsoleKey.LeftArrow) base.Move(VectorInt2.Left);
            if (k.Key == ConsoleKey.RightArrow) base.Move(VectorInt2.Right);
            if (k.Key == ConsoleKey.Backspace) base.UndoMove();
            
            return true;
        }


        private void Draw()
        {
            foreach (var tile in Current)
            {
                renderer[tile.Position.X, tile.Position.Y] = new CHAR_INFO(  themeChar[ tile.Cell.Underlying], theme[tile.Cell.Underlying]);
            }
            
            renderer.Box(new Rect(10, 10, 20, 20), new CHAR_INFO('@') );
            
            renderer.Update();
        }
    }
}