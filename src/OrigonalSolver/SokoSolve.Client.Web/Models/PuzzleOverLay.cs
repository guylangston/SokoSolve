using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.AspNetCore.Mvc.Rendering;
using SokoSolve.Core;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using SokoSolve.Drawing;
using SokoSolve.Drawing.SVG;

namespace SokoSolve.Client.Web.Models
{
    public class PuzzleOverLay
    {
        public PuzzleOverLay(Puzzle puzzle, IBitmap overlay)
        {
            Puzzle = puzzle;
            Overlay = overlay;

            Diagram = new PuzzleDiagram()
            {
                GetResource = x => "/img/"+x,
            };
            Diagram.GetOverlay = (t, r) =>
            {
                string s = "";
                if (Overlay[t.Position])
                {
                    s += new RectTag(r).Style(OverlayStyle);
                }
                return s;
            };
            OverlayStyle = new StyleTag().Fill(0, 180, 0).Opacity(0.4f);
        }

        public Puzzle Puzzle { get; set; }
        public PuzzleDiagram Diagram { get; set; }
        public Vector2 CellSize { get; set; }  = new Vector2(20);
        public IBitmap Overlay { get; set; }
        public StyleTag OverlayStyle { get; set; }
    }

    public static class DisplayHelper
    {

        public static IEnumerable<SelectListItem> ToSelectList<T>(
            IEnumerable<T> items,
            Func<T, (string value, string name)> convert,
            string selectedValue)
        {
            foreach (var item in items)
            {
                var x = convert(item);
                yield return new SelectListItem(x.name, x.value, string.Equals(selectedValue, x.value, StringComparison.InvariantCultureIgnoreCase));
            }
        }
    }
}
