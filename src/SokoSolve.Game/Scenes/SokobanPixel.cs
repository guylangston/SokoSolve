using System.Drawing;

namespace SokoSolve.Game.Scenes
{
    public class SokobanPixel // : ConsolePixel //  no inheritance for c# structs; but this is the idea
    {
        public SokobanPixel(char c)
        {
            Char = c;
            Fore = Color.Gray;
            Back = Color.Black;
        }
        public SokobanPixel(char c, Color fore, Color back, DisplayStyleElement? ext) 
        {
            Char = c;
            Fore = fore;
            Back = back;
            Ext  = ext;
        }
        
        public SokobanPixel(char c, DisplayStyleElement? ext) 
        {
            Char = c;
            Ext = ext;
            if (ext != null)
            {
                Fore = ext.Fore;
                Back = ext.Back;    
            }
        }

        public char                 Char { get; }
        public Color                Fore { get; }
        public Color                Back { get; }
        public DisplayStyleElement? Ext  { get;  }
    }
}