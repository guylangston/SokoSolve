using System.Collections.Generic;
using System.Linq;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Core.Game;
using VectorInt;

namespace SokoSolve.Console
{
    public class PuzzleGameLoop : GameLoopProxy
    {
        private Dictionary<char, CHAR_INFO_Attr> theme;
        private Dictionary<char, char> themeChar;
        
        public PuzzleGameLoop(GameLoopBase parent) : base(parent)
        {
        }

        public override void Init()
        {
            
            var Current = CellDefinition<CHAR_INFO>.Set;
            
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
            themeChar[Current.Definition.Wall.Underlying] = (char)0xB1;
            themeChar[Current.Definition.Void.Underlying] = ' ';
            themeChar[Current.Definition.Floor.Underlying] = ' ';
            themeChar[Current.Definition.Player.Underlying] = (char)0x02;
            themeChar[Current.Definition.PlayerGoal.Underlying] = (char)0x02;
            themeChar[Current.Definition.Crate.Underlying] = (char)0x15;
            themeChar[Current.Definition.CrateGoal.Underlying] = (char)0x7f;
        }

        public override void Step(float elapsedSec)
        {
        }

        public override void Draw()
        {
            var puzzle = new RectInt(0, 0, Current.Width, Current.Height);
            var pos = RectInt.CenterAt(renderer.Geometry.C, puzzle);
            
            renderer.Box(pos.Outset(2,2,2,2), RendererExt.AsciiBox );
            foreach (var tile in Current)
            {
                renderer[pos.TL + tile.Position] = new CHAR_INFO(themeChar[ tile.Value.Underlying], theme[tile.Value.Underlying]);
            }

            var txtStyle= new CHAR_INFO(' ', CHAR_INFO_Attr.FOREGROUND_GREEN | CHAR_INFO_Attr.FOREGROUND_INTENSITY);
            var txt = this.Elapsed.ToString();
            var txtPos = renderer.Geometry.TM - new VectorInt2(txt.Length /2, 0);
            renderer.DrawText(txtPos.X, txtPos.Y, txt, txtStyle );

            if (MousePosition.X > 0)
            {
                renderer.DrawText(0,0, MousePosition.ToString().PadRight(20), txtStyle);

                if (pos.Contains(MousePosition))
                {
                    var pz = MousePosition - pos.TL;
                    var pc = Current[pz];
                    renderer.DrawText(0,1, $"{pz} -> {pc.Underlying}".PadRight(40), txtStyle);
                }
                else
                {
                    renderer.DrawText(0,1, $"".PadRight(40), txtStyle);
                }
            }
            
            renderer.Update();
        }

        public override void Dispose()
        {
        }
    }
}