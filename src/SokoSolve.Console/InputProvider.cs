using System;
using System.Threading;
using System.Threading.Tasks;

namespace SokoSolve.Console
{
    public class InputProvider : IDisposable
    {
        private Task background;

        public InputProvider()
        {
            CancellationToken = new CancellationToken();
            background = Task.Run(DetectionLoop, CancellationToken);
        }
        
        public CancellationToken CancellationToken { get; }

        public bool IsKeyPressed(ConsoleKey key) => KeyDown[(byte) key];

        private bool[] KeyDown  = new bool[256];

        private void DetectionLoop()
        {
            while (!CancellationToken.IsCancellationRequested)
            {
                
            }
        }

        public void Dispose()
        {
        }
    }
}