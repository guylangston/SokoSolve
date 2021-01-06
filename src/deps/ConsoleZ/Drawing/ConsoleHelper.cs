using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ.Drawing
{
    public static class ConsoleHelper
    {
       public static IReadOnlyDictionary<ConsoleColor, Color> ToFullColor = new Dictionary<ConsoleColor, Color>()
       {
           {ConsoleColor.Black,       Color.Black},
           {ConsoleColor.DarkBlue,    Color.DarkBlue},
           {ConsoleColor.DarkGreen,   Color.DarkGreen},
           {ConsoleColor.DarkCyan,    Color.DarkCyan},
           {ConsoleColor.DarkRed,     Color.DarkRed},
           {ConsoleColor.DarkMagenta, Color.DarkMagenta},
           {ConsoleColor.DarkYellow,  Color.DarkOrange},
           {ConsoleColor.DarkGray,    Color.DarkGray},

           {ConsoleColor.Gray,        Color.Gray},
           {ConsoleColor.Blue,        Color.Blue},
           {ConsoleColor.Green,       Color.Green},
           {ConsoleColor.Cyan,        Color.Cyan},
           {ConsoleColor.Red,         Color.Red},
           {ConsoleColor.Magenta,     Color.Magenta},
           {ConsoleColor.Yellow,      Color.Yellow},
           {ConsoleColor.White,       Color.White}
       };

       public static IReadOnlyDictionary<Color, ConsoleColor> ToConsole = ToFullColor.ToDictionary(x => x.Value, x => x.Key);

       public static int WriteLine(this IConsole cons) => cons.WriteLine(null);
    }
}
