using System;
using System.IO;
using System.Numerics;
using SokoSolve.Core;
using SokoSolve.Drawing.SVG;
using VectorInt;

namespace SokoSolve.Drawing
{
    public class PuzzleDiagram : Diagram<Puzzle.Tile>
    {
        public string CrateSvg { get; set; } = "crate.svg";
        public string PlayerSvg { get; set; } = "player.svg";

        public PuzzleDiagram()
        {
            GetResource = x => x;
        }

        public Func<Puzzle.Tile, Rect2, string> GetOverlay { get; set; }

        public void Draw(TextWriter tw, Puzzle puzzle, Vector2 size)
        {
            this.Canvas = new Rect2(
                new Vector2(0),
                size * new Vector2(puzzle.Width, puzzle.Height));
            
            var transGrid = new TranslationRangeVector2(
                new TranslationRangeFloat(0f, (float)size.X, 0, this.Canvas.W),
                new TranslationRangeFloat(0f, (float)size.Y, 0, this.Canvas.H)
                ); 
                
            PlotTransform = new TranslationFunc<Puzzle.Tile, Rect2>(
                i =>
                {
                    return new Rect2((float)i.Position.X * size.X, (float)i.Position.Y * size.Y, size.X, size.Y);
                    //return new Rect2(transGrid.Translate(new Vector2(i.Position.X, i.Position.Y)), size);
                },
                o =>
                {
                    var p = transGrid.Inverse(o.TL);
                    return new Puzzle<char>.Tile(p, puzzle[p]);
                });

            DrawSVG(tw, puzzle, size);

        }

        public Func<string, string> GetResource { get; set; }

        private void DrawSVG(TextWriter tw, Puzzle puzzle, Vector2 size)
        {
            WriteHeader(tw);
            
            foreach (var tile in puzzle.ForEachTile())
            {
                var r = PlotTransform.Translate(tile);

                // if (tile.Cell == puzzle.Definition.Void)
                // {
                //     tw.WriteLine(new RectTag( r).Style(new StyleTag().Fill("black")));    
                // }
                if (tile.Cell == puzzle.Definition.Wall)
                {
                    tw.WriteLine(new ImageTag( r, GetResource("wall.svg")));    
                }
                if (tile.Cell.IsFloor)
                {
                    tw.WriteLine(new ImageTag( r, GetResource("floor.svg")));    
                }
                if (tile.Cell.IsGoal)
                {
                    tw.WriteLine(new ImageTag( r, GetResource("goal.svg")));    
                }
                if (tile.Cell.IsCrate && CrateSvg != null)
                {
                    tw.WriteLine(new ImageTag( r, GetResource(CrateSvg)));    
                }
                if (tile.Cell.IsPlayer && PlayerSvg != null)
                {
                    tw.WriteLine(new ImageTag( r, GetResource(PlayerSvg)));    
                }

                if (GetOverlay != null)
                {
                    var s = GetOverlay(tile, r);
                    if (s != null)
                    {
                        tw.WriteLine(s);
                    }
                }
                
                
                
            }

            WriteFooter(tw);
        }

        

        private void WriteHeader(TextWriter tw)
        {
             tw.WriteLine("<?xml version='1.0' standalone='yes'?>");
             tw.WriteLine($"<svg version='1.1' width='{Canvas.X2+1}' height='{Canvas.Y2+1}' xmlns='http://www.w3.org/2000/svg'>");
        }
        
        private void WriteFooter(TextWriter tw)
        {
            tw.WriteLine("</svg>");
        }
    }
}