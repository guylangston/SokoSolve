using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ConsoleZ.Drawing.Game;
using SkiaSharp;

namespace SokoSolve.Client.WPF
{
    public class WPFGameLoop : GameLoopBase
    {
        public GameScene Current { get; set; }

        public override void Init()
        {
            var h = (int)img.Height;
            var w = (int) img.Width;
            bmp = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            img.Source = bmp;
            
            int width  = (int)bmp.Width,
                height = (int)bmp.Height;

            surface = SKSurface.Create(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, bmp.BackBuffer, width * 4);

            
            renderer = new  SkiaConsolePixelRenderer(surface, tileSize, tileSize);

            Current?.Init();
        }

        private TextBlock txt;
        private Image img;
        private WriteableBitmap bmp;
        private SKSurface surface;
        private SkiaConsolePixelRenderer renderer;
        private DispatcherTimer dispatcher;
        private int dropped;
        private int tileSize;

        public SkiaConsolePixelRenderer Renderer => renderer;

        public WPFGameLoop(TextBlock txt, Image img, int tileSize)
        {
            this.txt = txt;
            this.img = img;
            this.tileSize = tileSize;
        }

        public override void Start()
        {
            IsActive = true;
            GameStarted = DateTime.Now;

            this.dispatcher = new DispatcherTimer(MinIntervalTimeSpan, DispatcherPriority.Render, (o, args) =>
            {
                if (IsActive)
                {
                    StartFrame = DateTime.Now;
                    Draw();
                    EndFrame = DateTime.Now;
                    //var elapse = (float)(EndFrame - StartFrame).TotalSeconds;

                    Step(MinIntervalSec);
                    
                    FrameCount++;

                    txt.Text = $"Act:{img.ActualWidth}x{img.ActualHeight}  vs Prop:{img.Width}x{img.Height}. Geo:{renderer.Geometry}, FPS:{FramesPerSecond,5:0.0}[{FrameCount,6}:{dropped}!]";
                }
            }, Dispatcher.CurrentDispatcher);
        }

        public override void Step(float elapsedSec)
        {
            Current?.Step(elapsedSec);
        }

        private volatile bool drawing;
        public override void Draw()
        {
            if (drawing) 
            {
                dropped++;
                return;
            };
            drawing = true;
            // https://lostindetails.com/articles/SkiaSharp-with-Wpf

            int width  = (int)bmp.Width,
                height = (int)bmp.Height;
            
            bmp.Lock();
        
            surface.Canvas.Clear(new SKColor(0, 0, 0));
            Current?.Draw();

            bmp.AddDirtyRect(new Int32Rect(0, 0, width, height));
            bmp.Unlock();
            drawing = false;


        }

        public override void Dispose()
        {
            surface?.Dispose();
        }
    }
}