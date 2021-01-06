using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ConsoleZ;
using ConsoleZ.DisplayComponents;
using ConsoleZ.Drawing;
using ConsoleZ.Samples;
using ConsoleZ.Win32;

namespace ConsoleZ.Playground.Console
{
    class Program
    {

        //static void Main(string[] args)
        //{
        //    var cons = AnsiConsole.Singleton;
        //    cons.WriteLine("Hello World");
        //    cons.WriteLine("Have a ^red;wonderful^; day!");
        //    var idx = cons.WriteLine("Replace me");
        //    cons.UpdateLine(idx, "I was replaced. ;-)");
        //}

        private static void Palette()
        {
            System.Console.OutputEncoding = Encoding.Unicode;
            DirectConsole.Setup(80, 25, 7*2, 14*2, "Consolas");
            //DirectConsole.MaximizeWindow();
            DirectConsole.Fill(' ',  0);


            var console = DirectConsole.Singleton;
            var renderer = new ConsoleRendererCHAR_INFO(console);

            ushort seed = 0xff00; 
            
            
            for (int x = 0; x < 16; x++)
            {
                console[x + 2, 0] = new CHAR_INFO(Convert.ToString(x, 16)[0], CHAR_INFO_Attr.FOREGROUND_GRAY);
                
                for(int y=0; y<16; y++)
                {
                    console[0, y + 2] = new CHAR_INFO(Convert.ToString(y, 16)[0], CHAR_INFO_Attr.FOREGROUND_GRAY);
                    console[1, y + 2] = new CHAR_INFO('x', CHAR_INFO_Attr.FOREGROUND_GRAY);

                    
                    console[x + 2, y + 2] = new CHAR_INFO('X', (ushort)(x + (16*y) + seed));
                }
            }

            for (int x = 0; x < 16; x++)
            {
                console[x + 22, 0] = new CHAR_INFO(Convert.ToString(x, 16)[0], CHAR_INFO_Attr.FOREGROUND_GRAY);
                
                for(int y=0; y<16; y++)
                {
                    console[20, y + 2] = new CHAR_INFO(Convert.ToString(y, 16)[0], CHAR_INFO_Attr.FOREGROUND_GRAY);
                    console[21, y + 2] = new CHAR_INFO('x', CHAR_INFO_Attr.FOREGROUND_GRAY);

                    
                    console[x + 22 , y + 2] = new CHAR_INFO()
                    { 
                        UnicodeNum = (ushort)(x + y*16 + seed) ,
                        AttributesEnum = CHAR_INFO_Attr.FOREGROUND_GRAY
                    };
                }
            }
            console.Update();

            System.Console.CursorTop = 20;
            System.Console.WriteLine("Hello World: " + (char)0xB1);
            System.Console.WriteLine("Done. Enter to exit...");

            System.Console.ReadLine();
        }

        static void Main(string[] args)
        {
            var cmd = args.Length > 0 ? args[0] : "default";
            
            System.Console.WriteLine($"Buffer: {System.Console.BufferWidth}x{System.Console.BufferHeight}. Window: {System.Console.WindowWidth}x{System.Console.WindowHeight}" );

            switch (cmd)
            {
                
                case "palette":
                    Palette();
                    break;
                
                default:
                case "simple":
                    Simple();
                    break;
            }
            
            //RunHeader();
            //RunBenchmark();
            //RunMarkDownSample();
            
            
            System.Console.WriteLine($"Buffer: {System.Console.BufferWidth}x{System.Console.BufferHeight}. Window: {System.Console.WindowWidth}x{System.Console.WindowHeight}" );
        }

        private static void Simple()
        {
            var cons = AnsiConsole.Singleton;
            SampleDocuments.DescribeConsole(cons); 
            SampleDocuments.MarkDownBasics(cons);
            SlowPlayback.LiveElements(cons);
            SampleDocuments.ColourPalette(cons);
        }

        private static void RunHeader()
        {
            //var cons = AnsiConsole.Singleton;
            //cons.WriteLine($"^cyan;TSDB^; Tools (TSDB: ^yellow;{0.1}^;)");
            //cons.WriteLine(null);
            //cons.WriteLine();
            //cons.WriteLine("Done");

            RunMarkDownSample();
            
        }

        private static void RunMarkDownSample()
        {
            var cons = AnsiConsole.Singleton;
            cons.UsePrefix = true;

            using (var fileTxt =
                new BufferedFileConsole(File.CreateText("e:\\Scratch\\console.txt"), "file", cons.Width, cons.Height)
                {
                    Renderer = new PlainConsoleRenderer()
                })
            {

                using (var fileHtml =
                    new BufferedFileConsole(File.CreateText("e:\\Scratch\\console.html"), "file", cons.Width,
                        cons.Height)
                    {
                        
                        Renderer = new HtmlConsoleRenderer()
                    })
                {
                    cons.Parent = fileTxt;
                    
                    fileTxt.Parent = fileHtml;

                    SampleDocuments.DescribeConsole(cons);

                    //foreach (var i in Enumerable.Range(0, 100))
                    //{
                    //    cons.WriteLine(i.ToString());
                    //}

                    //var ok = cons.UpdateLine(1, "XXX");
                    //cons.WriteLine($"Update -1 => {ok}");

                    SampleDocuments.MarkDownBasics(cons);
                    SlowPlayback.LiveElements(cons);
                    SampleDocuments.ColourPalette(cons);

                    
                   

                    SlowPlayback.LiveElementsFast(cons);


                    SampleDocuments.DescribeConsole(cons);
                    //var a = new ProgressBar(cons, "Test Scrolling").Start(100);
                    //for (int i = 0; i < a.ItemsTotal; i++)
                    //{
                    //    a.Increment(i.ToString());
                    //    Thread.Sleep(200);
                    //    cons.WriteLine(i.ToString());
                    //}
                    //a.Stop();
                }
            }
            

           
        }

        private static void RunBenchmark()
        {
            DirectConsole.Setup(80, 30, 16, 16, "Consolas");
            var screen = DirectConsole.Singleton;
            System.Console.WriteLine(Benchmark(2000, screen));
        }

        private static string Benchmark(int frameCount, IBufferedAbsConsole<CHAR_INFO> screen)
        {
            var r = new Random();
            var stop = new Stopwatch();
            stop.Start();
            for (int i = 0; i < frameCount; i++)
            {
                var x = r.Next(screen.Width);
                var y = r.Next(screen.Height);
                var a = r.Next(26);
                var clr = r.Next(0, 255);

                screen[x, y] = new CHAR_INFO(TestHelper.GetCharOffset('A', a), (ushort)clr);
                screen.Update();

                if (i % 50 == 0)
                {
                    System.Console.Title = i.ToString();
                }
            }

            stop.Stop();
            return $"{frameCount} frames in {stop.Elapsed} at {frameCount / stop.Elapsed.TotalSeconds} FPS";
        }
    }
}
