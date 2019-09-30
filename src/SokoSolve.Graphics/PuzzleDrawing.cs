using System;
using System.Collections.Generic;
using System.Drawing;

using System.IO;
using Sokoban.Core;
using Sokoban.Core.Primitives;
using Sokoban.Core.PuzzleLogic;

namespace Sokoban.Graphics
{
    public static class PuzzleDrawing
    {
        public class Theme
        {
            public VectorInt2 Offet { get; set; }
            public VectorInt2 Size { get; set; }
            public Dictionary<char, System.Drawing.Image> Cells { get; set; }

            public bool ShowVoid { get; set; }

            public static Theme Load(string s, int size)
            {
                var def = CellDefinition.Default;
                var cells = new Dictionary<char, Image>();

                cells.Add(def.Void, System.Drawing.Bitmap.FromFile(Path.Combine(s, "void.png")));
                cells.Add(def.Wall, System.Drawing.Bitmap.FromFile(Path.Combine(s, "wall.png")));
                cells.Add(def.Floor, System.Drawing.Bitmap.FromFile(Path.Combine(s, "floor.png")));
                cells.Add(def.Goal, System.Drawing.Bitmap.FromFile(Path.Combine(s, "goal.png")));
                cells.Add(def.Crate, System.Drawing.Bitmap.FromFile(Path.Combine(s, "crate.png")));
                cells.Add(def.Player, System.Drawing.Bitmap.FromFile(Path.Combine(s, "player.png")));

                return new Theme()
                {
                    Offet = VectorInt2.Zero,
                    Size = new VectorInt2(size, size),
                    Cells = cells
                };
            }
        }

        public static System.Drawing.Bitmap Draw(Puzzle puzzle, Theme theme)
        {
            var imgSize = theme.Size * new VectorInt2(puzzle.Width, puzzle.Height);
            var canvas = new System.Drawing.Bitmap(imgSize.X, imgSize.Y);
            foreach (var cell in puzzle)
            {
                foreach (var part in puzzle.Definition.Seperate(cell.State))
                {
                    Draw(puzzle, theme, cell, part, canvas);
                }
            }
            return canvas;
        }

        private static void Draw(Puzzle puzzle, Theme theme, Cell cell, char part, System.Drawing.Bitmap canvas)
        {
            if (!theme.ShowVoid && part == puzzle.Definition.Void) return;
            var pos = cell.Position * theme.Size + theme.Offet;
            var bit = theme.Cells[part];

            var g = System.Drawing.Graphics.FromImage(canvas);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            
            
            g.DrawImage(bit, pos.X, pos.Y, theme.Size.X, theme.Size.Y);
        }
    }
}
