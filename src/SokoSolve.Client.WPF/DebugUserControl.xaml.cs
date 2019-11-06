using System;
using System.Collections.Generic;
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
using SkiaSharp;

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
        
        private void DebugUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            var h = 1000;
            var bmp = new WriteableBitmap(h, h*3/2, 96, 96, PixelFormats.Bgra32, BitmapPalettes.Halftone256Transparent);
            img.Source = bmp;
            UpdateImage(bmp);
        }

        // https://lostindetails.com/articles/SkiaSharp-with-Wpf
        public void UpdateImage(WriteableBitmap writeableBitmap)
        {
            int width  = (int)writeableBitmap.Width,
                height = (int)writeableBitmap.Height;
            writeableBitmap.Lock();
            using (var surface = SKSurface.Create(width, height, SKColorType.Bgra8888, SKAlphaType.Premul, writeableBitmap.BackBuffer, width * 4))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(new SKColor(130, 130, 130));
                canvas.DrawText("SkiaSharp on Wpf!", 50, 200, new SKPaint() { Color = new SKColor(0, 0, 0), TextSize = 100 });
            }
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
            writeableBitmap.Unlock();
        }
    }
}
