using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ
{
    /// <summary>
    /// Box Chars
    /// https://jrgraphix.net/r/Unicode/2500-257F
    ///
    /// Wiki Box chars
    /// https://en.wikipedia.org/wiki/Box-drawing_character
    /// </summary>
    public static class UnicodeDrawing
    {
        // https://en.wikipedia.org/wiki/Block_Elements
        public const char Block100 = '█';
        public const char Block075 = '▓';
        public const char Block050 = '▒';
        public const char Block025 = '░';

        public const char BoxVert = '│';
        public const char BoxHorz = '─';
        public const char BoxTopLeft = '┌';
        public const char BoxTopRight = '┐';
        public const char BoxBottomLeft = '└';
        public const char BoxBottomRight = '┘';
        public const char BoxCross = '┼';
        public const char BoxVertLeft = '┤';
        public const char BoxVertRight = '├';
        public const char BoxHorzUp = '┴';
        public const char BoxHorzDown = '┬';

        public const char DotMiddle = '·';
        public const char Circle = '⬤';

    }
}
