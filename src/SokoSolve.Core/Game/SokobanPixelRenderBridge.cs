using System.Drawing;
using ConsoleZ.Drawing;
using ConsoleZ.Win32;
using SokoSolve.Core.Game.Scenes;

namespace SokoSolve.Core.Game
{
    public class SokobanPixelRenderBridge : RendererBridge<SokobanPixel, CHAR_INFO>
    {
        public SokobanPixelRenderBridge(IRenderer<CHAR_INFO> target) : base(target)
        {
        }

        protected override CHAR_INFO Convert(SokobanPixel a)
        {
            const byte min = 100;
         
            var c = new CHAR_INFO_Attr();
            c |= a.Fore.R > min ? CHAR_INFO_Attr.FOREGROUND_RED : 0;
            c |= a.Fore.G > min ? CHAR_INFO_Attr.FOREGROUND_GREEN : 0;
            c |= a.Fore.B > min ? CHAR_INFO_Attr.FOREGROUND_BLUE : 0;
            c |= a.Fore.GetBrightness() > 0.3f ? CHAR_INFO_Attr.FOREGROUND_INTENSITY : 0;            
            
            c |= a.Back.R > min ? CHAR_INFO_Attr.BACKGROUND_RED : 0;
            c |= a.Back.G > min ? CHAR_INFO_Attr.BACKGROUND_GREEN : 0;
            c |= a.Back.B > min ? CHAR_INFO_Attr.BACKGROUND_BLUE : 0;
            c |= a.Back.GetBrightness() > 0.3f ? CHAR_INFO_Attr.BACKGROUND_INTENSITY : 0;
            
            return new CHAR_INFO(a.Char, c);
        }

        protected override SokobanPixel Convert(CHAR_INFO a)
        {
            return new SokobanPixel(a.UnicodeChar, Color.Gray, Color.Black,  null);
        }
    }
}