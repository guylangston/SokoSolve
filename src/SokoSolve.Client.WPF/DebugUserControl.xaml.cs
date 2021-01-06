using System.Windows;
using System.Windows.Controls;
using ConsoleZ.Drawing;
using ConsoleZ.Samples;
using SokoSolve.Game;
using SokoSolve.Game.Scenes;

namespace SokoSolve.Client.WPF
{
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
            }
          
            
            gameLoop.Start();
        }

        
        public void Dispose()
        {
        }

        

        
    }
}
