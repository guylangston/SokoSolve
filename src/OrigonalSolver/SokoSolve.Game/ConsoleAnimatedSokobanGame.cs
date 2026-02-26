using ConsoleZ.Drawing;
using SokoSolve.Core;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib;
using SokoSolve.Game.Scenes;
using VectorInt;

namespace SokoSolve.Game
{
    public class ConsoleAnimatedSokobanGame : AnimatedSokobanGame
    {
        private          PlayPuzzleScene         parent;
        private DisplayStyle style;
        private readonly IRenderer<SokobanPixel> renderer;
        public           TutorialElement         Tutorial { get; set; }

        public ConsoleAnimatedSokobanGame(LibraryPuzzle puzzle, IRenderer<SokobanPixel> renderer, PlayPuzzleScene parent, DisplayStyle style) : base(puzzle)
        {
            this.renderer = renderer;
            this.parent   = parent;
            this.style = style;

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
                Paint = PaintMouse
            };
            Tutorial = new TutorialElement()
            {
                Paint = (el) =>
                {
                    if (Tutorial.CurrentMessageText != null)
                    {
                        renderer.DrawText((PuzzleSurface.ML.X / 2, PuzzleSurface.ML.Y), Tutorial.CurrentMessageText, style.Info.AsPixel(), TextAlign.Middle);
                    }
                }
            };
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

        public override void Init(Puzzle puzzle)
        {
            base.Init(puzzle);
            PuzzleSurface = RectInt.CenterAt(renderer.Geometry.C, puzzle.Area);

            AddAndInitElement(Tutorial);
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
                Paint       = PaintCell,
                ZIndex      = part.MemberOf.All.IndexOf(part)
            };
        }

        private void PaintMouse(GameElement _)
        {
            // Walk Path
            if (MouseMoveElement.WalkPath != null)
            {
                var s = Current.Player.Position + PuzzleSurface.TL;
                foreach (var pair in MouseMoveElement.WalkPath)
                {
                    s += pair;
                    renderer[s] = style.Mouse;
                }
            }

            // Overloys: Mouse
            if (parent.Input.IsMouseEnabled)
            {
                var mousePosition = parent.Input.MousePosition;
                var pz            = mousePosition - parent.GameLogic.PuzzleSurface.TL;
                var headerStyle   = parent.HeaderStyle;
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

        private void PaintCell(GameElement el)
        {
            if (el.Type.IsCrate)
            {
                var currCrate = Current[el.Position];
                renderer[el.Position + PuzzleSurface.TL] = style[currCrate];
            }
            else
            {
                renderer[el.Position + PuzzleSurface.TL] = style[el.Type];
            }

        }
    }
}
