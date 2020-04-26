using System.Collections.Generic;
using System.IO;
using System.Linq;
using SokoSolve.Core.Solver;
using TextRenderZ;

namespace SokoSolve.Drawing.GraphVis
{
    public class GraphVisRender
    {
        /// <summary>
        /// dot .\Exhause.dot -o file.svg -T svg
        /// </summary>
        
        public void Render(IEnumerable<SolverNode> items, TextWriter tw)
        {
            
            var sb = FluentString.Create()
                 .AppendLine("digraph{ rankdir=BT;")
                 .ForEach(items, (fb, x) =>
                 {
                     string lbl = x.SolverNodeId.ToString();
                     string bg = null;
                     var shape = "";
                     if (x.Parent == null) shape = "doublecircle";
                     else if (x.Status == SolverNodeStatus.Dead || x.Status == SolverNodeStatus.DeadRecursive)
                     {
                         shape = "square";
                         bg = "gray";
                     }
                     else if (x.Status == SolverNodeStatus.Solution) shape = "tripleoctagon";
                     else if (x.Status == SolverNodeStatus.SolutionPath) shape = "doubleoctagon";
                     else if (x.Status == SolverNodeStatus.Evaluted || x.Status == SolverNodeStatus.Evaluted) shape = "diamond";
                     else if (x.Status == SolverNodeStatus.Duplicate)
                     {
                         shape = "tab";
                         lbl += "->" + x.Duplicate?.SolverNodeId ?? "?";
                     }
                     else
                     {
                         shape = "cylinder";
                     }
                     
                     fb.Append($"{x.SolverNodeId} [label=\"{lbl}\" shape=\"{shape}\"");

                     if (bg != null)
                     {
                         fb.Append($" style=\"filled\", fillcolor=\"{bg}\"");
                     }
                     
                     fb.AppendLine("]");
                 })
                 .AppendLine("")
                 .ForEach(items.Where(x=>x.Parent != null), (fb, x) => fb.AppendLine( $"{x.SolverNodeId} -> {x.Parent?.SolverNodeId.ToString() ?? "null"}"))
                 .Append("}");
            
            tw.WriteLine(sb);
        } 
    }
}