using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Win32;
using SkiaSharp;

namespace SokoSolve.Client.WPF
{
    public class WPFGameLoop : RenderingGameLoopBase<ConsolePixel>
    {
        private TextBlock       txt;
        private Image           img;
        private WriteableBitmap bmp;
        private SKSurface       surface;
        private DispatcherTimer dispatcher;
        private int             dropped;
        private int             tileSize;

        public WPFGameLoop(IInputProvider inputProvider, TextBlock txt, Image img, int tileSize) : base(inputProvider, null)
        {
            this.txt = txt;
            this.img = img;
            this.tileSize = tileSize;
        }

        public IRenderingGameLoop<ConsolePixel> Scene { get; set; }

        public override void Init()
        {
            var h = (int)img.Height;
            var w = (int) img.Width;
            bmp        = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            img.Source = bmp;
            
            int width  = (int)bmp.Width,
                height = (int)bmp.Height;

            surface = SKSurface.Create(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, bmp.BackBuffer, width * 4);

            Renderer = new SkiaConsolePixelRenderer(surface, tileSize, tileSize);

            Scene?.Init();
        }

        public void Start()
        {
            IsActive = true;

            var frameTimer = new Stopwatch();
            frameTimer.Start();
            float elapsed = 0;
            this.dispatcher = new DispatcherTimer(
                TimeSpan.FromSeconds(base.FrameIntervalGoal), 
                DispatcherPriority.Render, 
                (o, args) =>
                {
                    
                    if (IsActive)
                    {
                        var last = (float)frameTimer.Elapsed.TotalSeconds - elapsed;
                        Step(last);
                        elapsed = (float)frameTimer.Elapsed.TotalSeconds;
                        Draw();
                        
                        FrameCount++;
                        txt.Text = $"Act:{img.ActualWidth}x{img.ActualHeight}  vs Prop:{img.Width}x{img.Height}. Geo:{Renderer.Geometry}, FPS:{FramesPerSecond,5:0.0}[{FrameCount,6}:{dropped}!]";
                    }
                },
                Dispatcher.CurrentDispatcher);
        }

        public override void Step(float elapsedSec)
        {
            Scene?.Step(elapsedSec);
            
            Input.Step(elapsedSec);     // this clears input; so do this last
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
            Scene?.Draw();

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