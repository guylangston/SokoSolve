using System;
using System.Collections.Generic;
using System.Transactions;
using ConsoleZ;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Console
{
    public class SimpleConsoleGameClient : SokobanGameLogic
    {

        public void Init()
        {
            System.Console.CursorSize = 1;
            System.Console.CursorVisible = false;
            
            DirectConsole.Setup(80, 25, 7*2, 14*2, "Courier New");
            DirectConsole.Fill(' ', 0);
            Current = Puzzle.Builder.DefaultTestPuzzle();
            
            // TODO: Mouse Interaction https://stackoverflow.com/questions/1944481/console-app-mouse-click-x-y-coordinate-detection-comparison
            
            theme = new Dictionary<char, byte>()
            {
                {Current.Definition.Void.Underlying,  (byte)ConsoleColor.Black},
                {Current.Definition.Wall.Underlying,  (byte)((byte)ConsoleColor.Blue + (byte)ConsoleColor.Cyan << 2)},
                {Current.Definition.Floor.Underlying,  (byte)(ConsoleColor.Gray)},
                {Current.Definition.Goal.Underlying,  (byte)(ConsoleColor.Blue)},
                {Current.Definition.Crate.Underlying,  (byte)(ConsoleColor.Yellow)},
                {Current.Definition.CrateGoal.Underlying,  (byte)(ConsoleColor.Yellow)},
                {Current.Definition.Player.Underlying,  (byte)(ConsoleColor.White)},
                {Current.Definition.PlayerGoal.Underlying,  (byte)(ConsoleColor.White)},
            };
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

        private Dictionary<char, byte> theme;

        private void Draw()
        {
            foreach (var tile in Current)
            {
                DirectConsole.Set(tile.Position.X, tile.Position.Y, tile.Cell.Underlying, theme[tile.Cell.Underlying]);
            }
            DirectConsole.Update();
        }
    }
}