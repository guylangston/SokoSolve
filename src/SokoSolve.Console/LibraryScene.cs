using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using SokoSolve.Core.Library;
using VectorInt;

namespace SokoSolve.Console
{
    public class LibraryScene : GameLoopProxy<MasterGameLoop>
    {
        private ConsoleRendererCHAR_INFO renderer;
        private Dictionary<char, CHAR_INFO_Attr> theme;
        private Dictionary<char, char> themeChar;
        
        public LibraryScene(MasterGameLoop parent, Library library, ConsoleRendererCHAR_INFO renderer) : base(parent)
        {
            Library = library;
            this.renderer = renderer;
        }

        Library Library { get; }
        private int PuzzleIndex { get; set; }
        public InputProvider Input => Parent.Input;
        
        public override void Init()
        {
            PuzzleIndex = 0;

            var def = Library[0].Puzzle.Definition;
            
            theme = new Dictionary<char, CHAR_INFO_Attr>()
            {
                {def.Void.Underlying,  CHAR_INFO_Attr.BLACK },
                {def.Wall.Underlying,  CHAR_INFO_Attr.BACKGROUND_GRAY},
                {def.Floor.Underlying, CHAR_INFO_Attr.FOREGROUND_GRAY },
                {def.Goal.Underlying,  CHAR_INFO_Attr.FOREGROUND_GRAY },
                {def.Crate.Underlying, CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.BACKGROUND_BLUE },
                {def.CrateGoal.Underlying,  CHAR_INFO_Attr.FOREGROUND_RED |  CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {def.Player.Underlying,   CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {def.PlayerGoal.Underlying,   CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY },
            };
            themeChar = def.ToDictionary(x => x.Underlying, x => x.Underlying);
            
            themeChar[def.Wall.Underlying] = (char)0xB1;
            themeChar[def.Void.Underlying] = ' ';
            themeChar[def.Floor.Underlying] = ' ';
            themeChar[def.Player.Underlying] = (char)0x02;
            themeChar[def.PlayerGoal.Underlying] = (char)0x02;
            themeChar[def.Crate.Underlying] = (char)0x15;
            themeChar[def.CrateGoal.Underlying] = (char)0x7f;
        }

        public override void Step(float elapsedSec)
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
        }

        public override void Draw()
        {
            renderer.Fill(new CHAR_INFO());
            
            // Center
            var selected = Library[PuzzleIndex];
            renderer.DrawLine(renderer.Geometry.TM, renderer.Geometry.BM, new CHAR_INFO('|'));
            var centerRect = RectInt.CenterAt(renderer.Geometry.C, selected.Puzzle.Area);
            var outer = centerRect.Outset(2, 2, 2, 2);
            renderer.Box(outer);
            DrawPuzzle(selected, centerRect, RenderCell);
            
            renderer.DrawText(outer.BM + (0, 2), selected.Name, new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY), TextAlign.Middle);
            renderer.DrawText(outer.TM + (0, -2), $"No. {PuzzleIndex+ 1}", new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY), TextAlign.Middle);
            
            // Right
            var r = centerRect.MR + (4,0);
            var ii = PuzzleIndex + 1;
            while (renderer.Geometry.Contains(r) && Library.Count > ii)
            {
                var nextP = Library[ii];
                var next = new RectInt(r.X, r.Y - nextP.Puzzle.Height/2, nextP.Puzzle.Width, nextP.Puzzle.Height);
                DrawPuzzle(nextP, next, RenderCell);

                r = next.MR + (2,0);
                ii++;
            }
            
            // Left
            r = centerRect.ML - (3,0);
            ii = PuzzleIndex - 1;
            while (renderer.Geometry.Contains(r) && ii >= 0)
            {
                var nextP = Library[ii];
                var next = new RectInt(r.X - nextP.Puzzle.Width, r.Y - nextP.Puzzle.Height/2, nextP.Puzzle.Width, nextP.Puzzle.Height);
                DrawPuzzle(nextP, next, RenderCell);

                r = next.ML - (2,0);
                ii--;
            }
            
            renderer.Update();
        }

        private CHAR_INFO RenderCell(CellDefinition<char> arg)
        {
            return new CHAR_INFO(themeChar[arg.Underlying], theme[arg.Underlying]);
        }

        private void DrawPuzzle(LibraryPuzzle libraryPuzzle, RectInt r, Func<CellDefinition<Char>, CHAR_INFO> getCell )
        {
            foreach (var (inner, outer) in r.InnerVsOuter())
            {
                renderer[outer] = getCell(libraryPuzzle.Puzzle[inner]);
            }
        }

        public override void Dispose()
        {
            
        }
    }
}