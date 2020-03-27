using System;
using System.Collections.Generic;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Lib;
using VectorInt;

namespace SokoSolve.Game.Scenes
{
    public class LibraryScene : GameScene<SokoSolveMasterGameLoop, SokobanPixel>
    {
        
        private Dictionary<char, CHAR_INFO_Attr> theme;
        private Dictionary<char, char> themeChar;
        
        public LibraryScene(SokoSolveMasterGameLoop parent, Library library) : base(parent)
        {
            Library = library;

//            var def = Library[0].Puzzle.Definition;
//            theme = new Dictionary<char, CHAR_INFO_Attr>()
//            {
//                {def.Void.Underlying,       CHAR_INFO_Attr.BLACK },
//                {def.Wall.Underlying,       CHAR_INFO_Attr.BACKGROUND_GRAY},
//                {def.Floor.Underlying,      CHAR_INFO_Attr.FOREGROUND_GRAY },
//                {def.Goal.Underlying,       CHAR_INFO_Attr.FOREGROUND_GRAY },
//                {def.Crate.Underlying,      CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.BACKGROUND_BLUE },
//                {def.CrateGoal.Underlying,  CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
//                {def.Player.Underlying,     CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
//                {def.PlayerGoal.Underlying, CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY },
//            };
//            themeChar = def.ToDictionary(x => x.Underlying, x => x.Underlying);
//            
//            themeChar[def.Wall.Underlying]       = (char)0xB1;
//            themeChar[def.Void.Underlying]       = ' ';
//            themeChar[def.Floor.Underlying]      = ' ';
//            themeChar[def.Player.Underlying]     = (char)0x02;
//            themeChar[def.PlayerGoal.Underlying] = (char)0x02;
//            themeChar[def.Crate.Underlying]      = (char)0x15;
//            themeChar[def.CrateGoal.Underlying]  = (char)0x7f;
        }

        Library Library { get; }
        private int PuzzleIndex { get; set; }
        
        
        public override void Init()
        {
            PuzzleIndex = 0;
        }

        public override void Step(float elapsedSec)
        {
            if (Input.IsKeyPressed())
            {
                if (Input.IsKeyPressed(ConsoleKey.RightArrow))
                {
                    PuzzleIndex++;
                    if (PuzzleIndex >=Library.Count) PuzzleIndex = 0;
                }
                if (Input.IsKeyPressed(ConsoleKey.LeftArrow))
                {
                    PuzzleIndex--;
                    if (PuzzleIndex < 0) PuzzleIndex = Library.Count - 1;
                }
            
                if (Input.IsKeyPressed(ConsoleKey.Enter))
                {
                    Parent.PlayPuzzle(Library[PuzzleIndex]);
                }
            
                if (Input.IsKeyPressed(ConsoleKey.S))
                {
                    Parent.Solve(Library[PuzzleIndex]);
                }
            }
            
        }

        private DisplayStyle Style => Parent.Style;

        public override void Draw()
        {
            // Center
            var selected = Library[PuzzleIndex];
            Renderer.DrawLine(Renderer.Geometry.TM, Renderer.Geometry.BM, Style.VerticalLine);
            var centerRect = RectInt.CenterAt(Renderer.Geometry.C, selected.Puzzle.Area);
            var outer = centerRect.Outset(2, 2, 2, 2);
            
            //Renderer.Box(outer);
            DrawPuzzle(selected, centerRect);
            
            Renderer.DrawText(outer.BM + (0, 2), selected.Name, Style.TextTitle.AsPixel(), TextAlign.Middle);
            Renderer.DrawText(outer.TM + (0, -2), $"No. {PuzzleIndex+ 1}", Style.TextHilight.AsPixel(), TextAlign.Middle);
            
            // Right
            var r = centerRect.MR + (4,0);
            var ii = PuzzleIndex + 1;
            while (Renderer.Geometry.Contains(r) && Library.Count > ii)
            {
                var nextP = Library[ii];
                var next = new RectInt(r.X, r.Y - nextP.Puzzle.Height/2, nextP.Puzzle.Width, nextP.Puzzle.Height);
                DrawPuzzle(nextP, next);

                r = next.MR + (2,0);
                ii++;
            }
            
            // Left
            r = centerRect.ML - (3,0);
            ii = PuzzleIndex - 1;
            while (Renderer.Geometry.Contains(r) && ii >= 0)
            {
                var nextP = Library[ii];
                var next = new RectInt(r.X - nextP.Puzzle.Width, r.Y - nextP.Puzzle.Height/2, nextP.Puzzle.Width, nextP.Puzzle.Height);
                DrawPuzzle(nextP, next);

                r = next.ML - (2,0);
                ii--;
            }
         
        }
        

        private void DrawPuzzle(LibraryPuzzle libraryPuzzle, RectInt r)
        {
            foreach (var (inner, outer) in r.InnerVsOuter())
            {
                Renderer[outer] =  Style[libraryPuzzle.Puzzle[inner]];
            }
        }

        public override void Dispose()
        {
            
        }
    }
}