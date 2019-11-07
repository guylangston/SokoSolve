using System;
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
using ConsoleZ.Samples;
using SkiaSharp;

namespace SokoSolve.Client.WPF
{
    /// <summary>
    /// Interaction logic for DebugUserControl.xaml
    /// </summary>
    public partial class DebugUserControl : UserControl
    {
        private WPFGameLoop gameLoop;

        public DebugUserControl()
        {
            InitializeComponent();
            
        }
        
        
        private void DebugUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.gameLoop = new WPFGameLoop(this.txt, this.img, 16);
            
            gameLoop.Init();
            
            gameLoop.Current = new SampleScene(gameLoop, gameLoop.Renderer);
            gameLoop.Current.Init();

            gameLoop.Start();
        }
        
    }
}
