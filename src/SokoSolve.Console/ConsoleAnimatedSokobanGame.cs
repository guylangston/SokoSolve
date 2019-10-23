using System.Collections.Generic;
using System.Linq;
using ConsoleZ.Drawing;
using ConsoleZ.Win32;
using SokoSolve.Core.Common;
using SokoSolve.Core.Game;
using SokoSolve.Core.Library;
using VectorInt;

namespace SokoSolve.Console
{
    public class ConsoleAnimatedSokobanGame : AnimatedSokobanGame
    {
        private readonly ConsoleRendererCHAR_INFO         renderer;
        private          AnimatedPuzzleGameLoop           parent;
        private          Dictionary<char, CHAR_INFO_Attr> theme;
        private          Dictionary<char, char>           themeChar;

        public ConsoleAnimatedSokobanGame(LibraryPuzzle puzzle, ConsoleRendererCHAR_INFO renderer, AnimatedPuzzleGameLoop parent) : base(puzzle)
        {
            this.renderer = renderer;
            this.parent   = parent;
            Text = new ConsoleElement()
            {
                Paint = (el) =>
                {
                    foreach (var (item, index) in Text.lines.WithIndex())
                    {
                        renderer.DrawText(renderer.Geometry.TL + (0, index + 2), item.Text, parent.DefaultStyle, TextAlign.Left);    
                    }
                }
            };
            MouseMoveElement = new MouseMoveElement(parent.Input)
            {
                Paint = MousePaint
            };

            var def = puzzle.Puzzle.Definition;
            theme = new Dictionary<char, CHAR_INFO_Attr>()
            {
                {def.Void.Underlying, CHAR_INFO_Attr.BACKGROUND_GRAY},
                {def.Wall.Underlying, CHAR_INFO_Attr.BACKGROUND_BLUE | CHAR_INFO_Attr.BACKGROUND_GREEN},
                {def.Floor.Underlying, CHAR_INFO_Attr.FOREGROUND_GRAY},
                {def.Goal.Underlying, CHAR_INFO_Attr.FOREGROUND_GRAY},
                {def.Crate.Underlying, CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.BACKGROUND_BLUE},
                {def.CrateGoal.Underlying, CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {def.Player.Underlying, CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
                {def.PlayerGoal.Underlying, CHAR_INFO_Attr.FOREGROUND_RED | CHAR_INFO_Attr.FOREGROUND_INTENSITY},
            };
            themeChar = def.ToDictionary(x => x.Underlying, x => x.Underlying);
            
            // https://www.fileformat.info/info/unicode/block/box_drawing/list.htm
            // http://www.fileformat.info/info/unicode/block/block_elements/images.htm
            themeChar[def.Wall.Underlying]       = (char)0xB1;
            themeChar[def.Void.Underlying]       = ' ';
            themeChar[def.Floor.Underlying]      = ' ';
            themeChar[def.Player.Underlying]     = (char)0x02;
            themeChar[def.PlayerGoal.Underlying] = (char)0x02;
            themeChar[def.Crate.Underlying]      = (char)0x15;
            themeChar[def.CrateGoal.Underlying]  = (char)0x7f;
        }

        public override void Draw()
        {
            base.Draw();
            
            renderer.DrawText(renderer.Geometry.TM, LibraryPuzzle.Name, parent.HeaderStyle, TextAlign.Middle);
            renderer.DrawText(renderer.Geometry.TM + (0, 1), $"Difficulty rated: {LibraryPuzzle.Rating}", parent.InfoStyle, TextAlign.Middle);


            var line = renderer.Geometry.TR;
            renderer.DrawText(line, "Steps", parent.HeaderStyle, TextAlign.Right);
            line += (0, 1);
            renderer.DrawText(line, Statistics.Steps.ToString(), parent.InfoStyle, TextAlign.Right);
            line += (0, 2);
            
            renderer.DrawText(line, "Pushes", parent.HeaderStyle, TextAlign.Right);
            line += (0, 1);
            renderer.DrawText(line, Statistics.Pushes.ToString(), parent.InfoStyle, TextAlign.Right);
            line += (0, 2);
            
            renderer.DrawText(line, "Undos", parent.HeaderStyle, TextAlign.Right);
            line += (0, 1);
            renderer.DrawText(line, Statistics.Undos.ToString(), parent.InfoStyle, TextAlign.Right);
            line += (0, 2);
            
            renderer.DrawText(line, "Restarts", parent.HeaderStyle, TextAlign.Right);
            line += (0, 1);
            renderer.DrawText(line, Statistics.Restarts.ToString(), parent.InfoStyle, TextAlign.Right);
            line += (0, 2);
            
            
            renderer.DrawText(renderer.Geometry.BM, $"{Statistics.Elapased.TotalSeconds:0.0} sec elapsed", parent.InfoStyle, TextAlign.Middle);
        }

        private void MousePaint(GameElement _)
        {
            // Walk Path
            if (MouseMoveElement.WalkPath != null)
            {
                var s = Current.Player.Position + PuzzleSurface.TL;
                foreach (var pair in MouseMoveElement.WalkPath)
                {
                    s += pair;
                    renderer[s] = new CHAR_INFO('*', CHAR_INFO_Attr.BACKGROUND_BLUE | CHAR_INFO_Attr.FOREGROUND_GREEN);
                }
            }
            
         
            
            // Overloys: Mouse
            if (parent.Input.IsMouseEnabled)
            {
                var mousePosition = parent.Input.MousePosition;
                var pz            = mousePosition - parent.GameLogic.PuzzleSurface.TL;
                var headerStyle = parent.HeaderStyle;
                renderer.DrawText(0, 0, mousePosition.ToString().PadRight(20), headerStyle);

                if (parent.GameLogic.PuzzleSurface.Contains(mousePosition))
                {
                    var pc = parent.GameLogic.Current[pz];
                    renderer.DrawText(0, 1, $"{pz} -> {pc.Underlying}".PadRight(40), headerStyle);
                }
                else
                {
                    renderer.DrawText(0, 1, $"".PadRight(40), headerStyle);
                }

                var start = renderer.Geometry.BL;
                foreach (var (item, index) in parent.GameLogic.ElementsAt(pz).WithIndex())
                {
                    renderer.DrawText(start - (0, index), item.ToString(), headerStyle);
                }
            }
        }

        public override void Init(Puzzle puzzle)
        {
            base.Init(puzzle);
            PuzzleSurface = RectInt.CenterAt(renderer.Geometry.C, puzzle.Area);
        }

        protected override GameElement Factory(CellDefinition<char> part, VectorInt2 startState)
        {
            return new GameElement()
            {
                Position    = startState,
                PositionOld = startState,
                StartState  = startState,
                Game        = this,
                Type        = part,
                Paint       = DefaultPaint,
                ZIndex      = part.MemberOf.All.IndexOf(part)
            };
        }

        private void DefaultPaint(GameElement el)
        {
            renderer[el.Position + PuzzleSurface.TL] = new CHAR_INFO(themeChar[el.Type.Underlying], theme[el.Type.Underlying]);
        }
    }
}