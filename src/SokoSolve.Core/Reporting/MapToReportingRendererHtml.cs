using System.Collections.Generic;
using System.IO;

namespace SokoSolve.Core.Reporting
{
    public class MapToReportingRendererHtml : IMapToReportingRenderer
    {
        public string TableClass { get; set; } = "table table-sm";
        
        public void Render<T>(IMapToReporting<T> mapping, IEnumerable<T> items, TextWriter outp)
        {
            outp.WriteLine($"<table class='{TableClass}'>");
            outp.WriteLine("<thead><tr>");
            
            foreach (var col in mapping.Columns)
            {
                outp.WriteLine($"<th title='{col.Description}'>{col.Title}</th>");    
            }
            outp.WriteLine("</tr></thead>");

            foreach (var item in mapping.GetRows(items))
            {
                outp.WriteLine("<tr>");
            
                foreach (var cell in item)
                {
                    outp.WriteLine($"<td>{cell.ValueDisplay}</th>");    
                }
                outp.WriteLine("</tr>");
            }
            outp.WriteLine("</table>");
        }
    }
}