using System.Drawing;
using System.Runtime.InteropServices;
using SokoSolve.Core;

namespace SokoSolve.Game.Scenes
{
    public class DisplayStyleElement
    {
        public DisplayStyleElement()
        {
            Fore = Color.Gray;
            Back = Color.Black;
        }

        public DisplayStyleElement(Color fore, Color back)
        {
            Fore = fore;
            Back = back;
        }

        public Color Fore { get; }
        public Color Back { get; }

        public SokobanPixel AsPixel(char c = ' ') => new SokobanPixel(c, Fore, Back, this);
    }
    
    public class DisplayStyle
    {
        public DisplayStyleElement Default     { get; set; } = new DisplayStyleElement();
        public DisplayStyleElement TextDefault { get; set; } = new DisplayStyleElement();
        public DisplayStyleElement Info        { get; set; } = new DisplayStyleElement(Color.Cyan, Color.Black);
        public DisplayStyleElement Error       { get; set; } = new DisplayStyleElement(Color.Red, Color.DarkBlue);
        public DisplayStyleElement TextTitle   { get; set; } = new DisplayStyleElement(Color.Yellow, Color.Black);
        public DisplayStyleElement TextHilight { get; set; } = new DisplayStyleElement(Color.Purple, Color.Black);
        
        
        public Color Fore { get; set; } = Color.Gray;
        public Color Back { get; set; } = Color.Black;

        public SokobanPixel DefaultPixel => new SokobanPixel(' ', Default);
        public SokobanPixel VerticalLine => new SokobanPixel('|', Default);
        public SokobanPixel Mouse        => new SokobanPixel('*', Default);

        public SokobanPixel this[CellDefinition<char> c]
        {
            get
            {
                if (c == c.MemberOf.Wall) return new SokobanPixel(c.Underlying,  Wall );
                if (c == c.MemberOf.Void) return new SokobanPixel(c.Underlying,  Void );
                if (c == c.MemberOf.Floor) return new SokobanPixel(c.Underlying,  Floor );
                if (c == c.MemberOf.Goal) return new SokobanPixel(c.Underlying,  Goal);
                
                if (c == c.MemberOf.Crate) return new SokobanPixel(c.Underlying,  Crate);
                if (c == c.MemberOf.CrateGoal) return new SokobanPixel(c.Underlying,  CrateGoal);
                
                if (c == c.MemberOf.Player) return new SokobanPixel(c.Underlying,  Player);
                if (c == c.MemberOf.PlayerGoal) return new SokobanPixel(c.Underlying,  PlayerGoal);
                
                return new SokobanPixel(c.Underlying, Default);
            }
        }

        public DisplayStyleElement Player     { get; set; } = new DisplayStyleElement(Color.Red, Color.DarkCyan);
        public DisplayStyleElement PlayerGoal { get; set; } = new DisplayStyleElement(Color.Red, Color.Green);
        public DisplayStyleElement Crate      { get; set; } = new DisplayStyleElement(Color.LightPink, Color.DarkCyan);
        public DisplayStyleElement CrateGoal  { get; set; } = new DisplayStyleElement(Color.LightPink, Color.Green);
        public DisplayStyleElement Goal       { get; set; } = new DisplayStyleElement(Color.GreenYellow, Color.Green);
        public DisplayStyleElement Floor      { get; set; } = new DisplayStyleElement(Color.Black, Color.DarkCyan);

        public DisplayStyleElement Void { get; set; } =
            new DisplayStyleElement(Color.GreenYellow, Color.DarkBlue);

        public DisplayStyleElement Wall { get; set; } =
            new DisplayStyleElement(Color.SaddleBrown, Color.DarkBlue);
    }
}