using System;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
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
using ConsoleZ.Win32;
using SkiaSharp;
using SokoSolve.Core.Game;
using SokoSolve.Core.Game.Scenes;
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

            var text = "sokoban";
            switch (text)
            {
                case "sample":
                    gameLoop.Scene = new SampleScene(gameLoop);
                    gameLoop.Init();
                    break;
                
                case "sokoban":
                    // Init, so that we get the renderer
                    gameLoop.Init();
                    
                    var bridgeSokobanPixelToConsolePixel = new BridgeSokobanPixelToConsolePixel(gameLoop.Renderer);
                    
                    var host = new BridgeGameLoop<ConsolePixel, SokobanPixel>(gameLoop, bridgeSokobanPixelToConsolePixel);
                    host.Inner = new SokoSolveMasterGameLoop(host);
                    
                    host.Init();
                    
                    
                    gameLoop.Scene = host;
                    
                    break;
                
                case "tetris":
                    var tetris = new MasterGameLoop(gameLoop);
                    gameLoop.Scene = tetris;
                    gameLoop.Init();
                    break;
            }
          
            
            gameLoop.Start();
        }

        
        public void Dispose()
        {
        }

        

        
    }
}
