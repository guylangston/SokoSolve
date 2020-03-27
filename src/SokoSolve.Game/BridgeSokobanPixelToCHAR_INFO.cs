using System;
using System.Drawing;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SokoSolve.Game.Scenes;

namespace SokoSolve.Game
{
    public class BridgeSokobanPixelToCHAR_INFO : RendererBridge<SokobanPixel, CHAR_INFO>
    {
        public BridgeSokobanPixelToCHAR_INFO(IRenderer<CHAR_INFO> target) : base(target)
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
    
    public class BridgeSokobanPixelToConsolePixel : RendererBridge<SokobanPixel, ConsolePixel>
    {
        public BridgeSokobanPixelToConsolePixel(IRenderer<ConsolePixel> target) : base(target)
        {
        }

        protected override ConsolePixel Convert(SokobanPixel a)
        {    
            return new ConsolePixel(a.Char, a.Fore,a.Back);
        }

        protected override SokobanPixel Convert(ConsolePixel a)
        {
            return new SokobanPixel(a.Char, Color.Gray, Color.Black,  null);
        }
    }
    
    public class BridgeGameLoop<TA, TB> : GameScene<IRenderingGameLoop<TA>, TA>, IRenderingGameLoop<TB>
    {
        public BridgeGameLoop(IRenderingGameLoop<TA> parent, IRenderer<TB> bridge) : base(parent)
        {
            this.bridge = bridge;
        }

        IRenderer<TB> bridge;

        public IRenderingGameLoop<TB>? Inner { get; set; }

        IRenderer<TB> IRenderingGameLoop<TB>.Renderer => bridge;

        public override void Init() => Inner?.Init();

        public override void Step(float elapsedSec) => Inner?.Step(elapsedSec);

        public override void Draw() => Inner?.Draw();

        public override void Dispose()
        {
            if (Inner is IDisposable dp) dp.Dispose();
        }

         
    }
}