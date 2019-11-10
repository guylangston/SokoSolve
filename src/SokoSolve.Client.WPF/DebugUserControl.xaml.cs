using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ConsoleZ.Drawing;
using ConsoleZ.Samples;
using SkiaSharp;
using Tetris.Lib.Rendering;
using VectorInt;

namespace SokoSolve.Client.WPF {
    public partial class DebugUserControl : UserControl
    {
        private WPFGameLoop gameLoop;

        public DebugUserControl()
        {
            InitializeComponent();
        }

      
        
        public WPFInputProvider InputProvider { get; set; }
        
        private void DebugUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            InputProvider = new WPFInputProvider();
            this.gameLoop = new WPFGameLoop(InputProvider,  this.txt, this.img, (int)(img.Width / 80));
            
            //gameLoop.Scene = new SampleScene(gameLoop);
            var tetris = new MasterGameLoop(gameLoop);
            gameLoop.Scene = tetris;
            gameLoop.Init();
            
            gameLoop.Start();
        }

        
        public void Dispose()
        {
        }

       

        
    }
}
