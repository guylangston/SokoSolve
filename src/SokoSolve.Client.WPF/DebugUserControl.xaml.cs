using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ConsoleZ.Drawing;
using ConsoleZ.Drawing.Game;
using ConsoleZ.Samples;
using SkiaSharp;
using VectorInt;
using Color = System.Drawing.Color;

namespace SokoSolve.Client.WPF
{
    /// <summary>
    /// Interaction logic for DebugUserControl.xaml
    /// </summary>
    public partial class DebugUserControl : UserControl
    {
        public DebugUserControl()
        {
            InitializeComponent();

        }
        private SampleScene scene;
        private SKSurface surface;
        private WriteableBitmap writeableBitmap;
        
        private void DebugUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var h = 1000;
            var bmp = new WriteableBitmap(h, h*3/2, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            img.Source = bmp;
            writeableBitmap = bmp;
            
            int width  = (int)bmp.Width,
                height = (int)bmp.Height;

            var size = 50;
            surface = SKSurface.Create(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, bmp.BackBuffer,
                width * 4);
            
            scene = new SampleScene(new SimpleGameLoop(), new  SkiaConsolePixelRenderer(surface, size, size));
            scene.Init();
            UpdateImage();

            var t = new DispatcherTimer(TimeSpan.FromSeconds(1 / 60f), DispatcherPriority.Render, (o, args) =>
            {
                scene.Step(1/60f);
                UpdateImage();
            }, Dispatcher.CurrentDispatcher);
        }

      
        // https://lostindetails.com/articles/SkiaSharp-with-Wpf
        public void UpdateImage()
        {
           
            int width  = (int)writeableBitmap.Width,
                height = (int)writeableBitmap.Height;

           
            writeableBitmap.Lock();
        
                SKCanvas canvas = surface.Canvas;
                
                canvas.Clear(new SKColor(130, 130, 130));
                
                
                scene.Draw();

//                canvas.DrawText("SkiaSharp on Wpf!", 50, 200, new SKPaint() { Color = new SKColor(0, 0, 0), TextSize = 100 });

                
        
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writeableBitmap.Unlock();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            scene.Step(1);
            UpdateImage();
        }
    }
    
    public class SimpleGameLoop : GameLoopBase
    {
        public override void Init()
        {
                
        }

        public override void Step(float elapsedSec)
        {
                
        }

        public override void Draw()
        {
                
        }

        public override void Dispose()
        {
                
        }
    }

    public class SkiaConsolePixelRenderer : SkiaSharpTileRenderer<ConsolePixel>
    {
        public SkiaConsolePixelRenderer(SKSurface surface, int cellWidth, int cellHeight) : base(surface, cellWidth, cellHeight)
        {
        }

        protected override void DrawTile(VectorInt2 p, ConsolePixel tile)
        {
            surface.Canvas.DrawRect(p.X * CellWidth, p.Y * CellHeight, CellWidth, CellHeight, GetSKPaint(tile.Back));
                
            surface.Canvas.DrawText(tile.Char.ToString(), p.X * CellWidth , p.Y * CellHeight + CellHeight, GetSKPaint(tile.Fore));
        }

        private Dictionary<Color, SKPaint> clr = new Dictionary<Color, SKPaint>(); 
            
        private SKPaint GetSKPaint(in Color tileBack)
        {
            return new SKPaint()
            {
                //Color    = new SKColor(tileBack.R, tileBack.G, tileBack.B, tileBack.A),
                Color = new SKColor(tileBack.R, tileBack.G, tileBack.B),
                TextSize = CellWidth,
            };
        }

        protected override void DrawTile(VectorInt2 p, char chr, ConsolePixel style)
        {
                
        }

        public override void DrawText(int x, int y, string txt, ConsolePixel style)
        {
                
        }
    }
}
