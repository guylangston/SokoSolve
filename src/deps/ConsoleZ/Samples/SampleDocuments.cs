using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConsoleZ.Drawing;

namespace ConsoleZ.Samples
{
    public static class SampleDocuments
    {

        public static void MarkDownBasics(IConsole console)
        {
            // https://guides.github.com/features/mastering-markdown/
            console.WriteLine("# Header");
            console.WriteLine("");
            console.WriteLine(
                "It's very easy to make some words **bold** and other words *italic* with Markdown. You can even [link to Google!](http://google.com) ");

            console.WriteLine(
                "Example Paragraph... Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam sit amet ligula interdum, placerat turpis nec, volutpat dolor. Nam egestas felis ac malesuada iaculis. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sit amet luctus mi. Maecenas et erat tristique, pellentesque mi vel, varius tellus. Duis consectetur vestibulum ipsum eget varius. Etiam tristique neque at est porttitor auctor quis eu purus. Duis vehicula leo ut sem varius, non molestie dolor malesuada. Praesent ut diam et dui interdum semper sed eget magna. Nullam sit amet finibus dolor, sit amet molestie quam. Interdum et malesuada fames ac ante ipsum primis in faucibus. Ut consequat tortor non iaculis gravida. Curabitur tempor erat ut laoreet auctor. ");

            console.WriteLine("List:");
            console.WriteLine("- Item One");
            console.WriteLine("- Item Two");
            console.WriteLine("- Item Three");

            console.WriteLine(@"But I have to admit, tasks lists are my favorite:

- [x] This is a complete item
- [ ] This is an incomplete item");


            console.WriteLabel(DateTime.Now, x => x.DayOfWeek);
            console.WriteLabel(DateTime.Now, x => x.Hour);


        }


        public static void ColourPalette(IConsole console)
        {
            // https://guides.github.com/features/mastering-markdown/
            console.WriteLine("# Colours");
            console.WriteLine("");

            foreach (var color in ConsoleHelper.ToConsole)
            {
                console.WriteLine($"{color.Key.Name,12} => ^{color.Key.Name};XXXX...^; reverted.");
            }
            console.WriteLine($"{"NotAColour",12} => ^NotAColour;XXXX...^; reverted.");
            

        }

        public static void DescribeConsole(AnsiConsole cons)
        {
            cons.WriteLabel("Title", cons.Title);
            cons.WriteLabel("Width", cons.Width);
            cons.WriteLabel("Height", cons.Height);
            cons.WriteLabel("DisplayStart", cons.DisplayStart);
            cons.WriteLabel("DisplayEnd", cons.DisplayEnd);
            cons.WriteLabel("Console", $"Buffer:{Console.BufferWidth}x{Console.BufferHeight}, " +
                                       $"Window:{Console.WindowWidth}x{Console.WindowHeight} @ {Console.WindowLeft}x{Console.WindowTop}, " +
                                       $"Buffer:{Console.BufferWidth}, Cursor:{Console.CursorLeft}x{Console.CursorTop}");
        }
    }
}
