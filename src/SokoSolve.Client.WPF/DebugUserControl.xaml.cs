using System;
using System.Configuration;
using System.Linq;
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
using ConsoleZ.Samples;
using ConsoleZ.Win32;
using SkiaSharp;
using Tetris.Lib.Rendering;
using VectorInt;

namespace SokoSolve.Client.WPF {
    
    /// </summary>
    public partial class DebugUserControl : UserControl, IInputProvider
    {
        private WPFGameLoop gameLoop;

        public DebugUserControl()
        {
            InitializeComponent();
        }

        public bool IsMouseEnabled { get; set; }
        public VectorInt2 MousePosition  { get; set; }
        public bool IsMouseClick => false;

        public bool[] keys = new bool[256];

        public void CaptureKeyPress(Key key)
        {
            keys[(int) ToConsoleKey(key)] = true;
        }

        public bool IsKeyDown(ConsoleKey key) => keys[(int)key];

        public bool IsKeyPressed() => keys.Any(x => x);

        public bool IsKeyPressed(ConsoleKey key) => keys[(int)key];

        public void Step(float elapsed)
        {
            Array.Fill(keys, false);
        }
        
        private void DebugUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.gameLoop = new WPFGameLoop(this,  this.txt, this.img, 10);
            
            //gameLoop.Scene = new SampleScene(gameLoop);
            var tetris = new MasterGameLoop(gameLoop);
            gameLoop.Scene = tetris;
            gameLoop.Init();
            
            gameLoop.Start();
        }

        
        public void Dispose()
        {
        }

        private void DebugUserControl_OnKeyDown(object sender, KeyEventArgs e)
        {
            keys[(int) ToConsoleKey(e.Key)] = true;
        }

        private ConsoleKey ToConsoleKey(Key eKey)
        {
            return eKey switch
            {
                Key.D1 => ConsoleKey.D1,
                Key.D2 => ConsoleKey.D2,
                

                Key.Left  => ConsoleKey.LeftArrow,
                Key.Right => ConsoleKey.RightArrow,
                Key.Up    => ConsoleKey.UpArrow,
                Key.Down  => ConsoleKey.DownArrow,

                Key.Enter  => ConsoleKey.Enter,
                Key.Escape => ConsoleKey.Escape,

                _ => ConsoleKey.F24

            };
        }
    }
}
