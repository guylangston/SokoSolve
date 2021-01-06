using System.Drawing;

namespace ConsoleZ.Drawing
{
    public struct ConsolePixel
    {
        public ConsolePixel(char c, Color fore, Color back)
        {
            Char = c;
            Fore = fore;
            Back = back;
        }

        public char  Char { get; }
        public Color Fore { get; }
        public Color Back { get; }
    }

    public struct ConsolePixel<T>    // : ConsolePixel //  no inheritance for c# structs; but this is the idea
    {
        public ConsolePixel(char c, Color fore, Color back, T ext)
        {
            Char = c;
            Fore = fore;
            Back = back;
            Ext = ext;
        }

        public char  Char { get; }
        public Color Fore { get; }
        public Color Back { get; }
        public T     Ext  { get; }
    }
}