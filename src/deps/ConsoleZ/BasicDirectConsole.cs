using System;
using ConsoleZ.Drawing;

namespace ConsoleZ
{
    public class BasicDirectConsole : IBufferedAbsConsole<ConsolePixel>
    {
        private ConsolePixel[,] buffer;
        
        private BasicDirectConsole()
        {
            Init();    
        }
        
        private static BasicDirectConsole singleton = new BasicDirectConsole();
        public static BasicDirectConsole Singleton => singleton;

        public void Init()
        {
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            buffer = new ConsolePixel[Console.WindowWidth, Console.WindowHeight];
        }

        public string Handle => "dotnet-console";
        public int Width => buffer.GetLength(0);
        public int Height => buffer.GetLength(1);

        public ConsolePixel this[int x, int y]
        {
            get => buffer[x,y];
            set
            {
                if (x < 0 || y < 0) return;
                if (x >= buffer.GetLength(0) || y >= buffer.GetLength(1)) return;
                buffer[x, y] = value;
            }
        }

        public void Fill(ConsolePixel fill)
        {
            for (int x = 0; x < buffer.GetLength(0); x++)
                for (int y = 0; y < buffer.GetLength(1); y++)
                {
                    buffer[x, y] = fill;
                }
        }

        public void Update()
        {
            // TODO: Set the cursor position once, then write a string the exact size of the screen...
            
            for (int y = 0; y < buffer.GetLength(1); y++)
            {
                Console.SetCursorPosition(0, y);
                for (int x = 0; x < buffer.GetLength(0) - 1; x++)
                {
                    Console.Write(buffer[x, y].Char);
                }
            }
        }
    }
}