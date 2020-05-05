using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using SokoSolve.Core.Components;
using SokoSolve.Core.Solver;
using TextRenderZ.Reporting;

namespace SokoSolve.Console
{
    internal class AnalyseCommand
    {
          
        public static Command GetCommand()
        {
            var analyse = new Command("analyse", "Analyse an .sbn file")
            {
                new Argument<string>()
                {
                    Name        = "file",
                    Description = "File to load an analyse"
                },
                new Option<string>(new[] {"--report", "-r"}, "Report to Run")
                {
                    Name = "report",
                },
                
            };
            analyse.Handler = CommandHandler.Create<string, string>(Run); 
            return analyse;
        }

        public static void Run(string file, string report)
        {
            System.Console.WriteLine($"<file> {file} --report {report}");


            var ser = new BinaryNodeSerializer();
            if (report == "depth")
            {
                using (var f = System.IO.File.OpenRead(file))
                {
                    using (var br = new BinaryReader(f))
                    {
                        System.Console.WriteLine($"Reading File...");
                        var root = ser.AssembleTree(br);
                        
                        

                        var repDepth = MapToReporting.Create<SolverHelper.DepthLineItem>()
                                                     .AddColumn("Depth", x => x.Depth)
                                                     .AddColumn("Total", x => x.Total)
                                                     .AddColumn("Growth Rate", x => x.GrowthRate)
                                                     .AddColumn("UnEval", x => x.UnEval)
                                                     .AddColumn("Complete", x => (x.Total - x.UnEval) *100 / x.Total, c=>c.ColumnInfo.AsPercentage());
                        repDepth.RenderTo(SolverHelper.ReportDepth(root), new MapToReportingRendererText(), System.Console.Out);
                    }
                }
            }
            else if (report == "clash")
            {
                using (var f = File.OpenRead(file))
                {
                    var writer = new BinaryNodeSerializer();
                    var nodes  = writer.ReadAll(new BinaryReader(f));

                    Dictionary<int, int> hash = new Dictionary<int, int>();
                    foreach (var n in nodes)
                    {
                        if (hash.TryGetValue(n.HashCode, out var c))
                        {
                            hash[n.HashCode] = c + 1;
                        }
                        else
                        {
                            hash[n.HashCode] = 1;
                        }
                    }

                    foreach (var pair in hash.OrderByDescending(x=>x.Value).Take(20))
                    {
                        System.Console.WriteLine(pair.ToString());
                    }
                }
            }
            else if (report == "dump")
            {
                using (var f = File.OpenRead(file))
                {
                    var writer = new BinaryNodeSerializer();
                    using (var br = new BinaryReader(f))
                    {
                        foreach (var node in writer.ReadAll(br).OrderBy(x=>x.SolverNodeId).Take(20))
                        {
                            System.Console.WriteLine(node);
                        }        
                    }
                }
            }
            else
            {
                throw new Exception($"Unknown Report: {report}");
            }


        
            
            

           
            
            
        }
    }
}