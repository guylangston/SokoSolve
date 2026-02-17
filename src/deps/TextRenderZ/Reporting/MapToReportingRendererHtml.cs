using System.Collections.Generic;
using System.IO;

namespace TextRenderZ.Reporting
{

    public class MapToReportingRendererHtml : IMapToReportingRenderer
    {
        public string TableClass { get; set; } = "table table-sm";
        public ICellFormatter? CellFormatter { get; set; }

        public MapToReportingRendererHtml(ICellFormatter cellFormatter)
        {
            CellFormatter = cellFormatter;
        }

        public MapToReportingRendererHtml() : this(new CellFormatter()) { }

        public void Render<T>(IMapToReporting<T> mapping, IEnumerable<T> items, ITextWriterAdapter outp)
        {
            if (CellFormatter == null)
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
            else
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
                        var tag = new CellContainerTag("td", null, null);
                        CellFormatter.WriteCell(outp, cell, ref tag);
                    }
                    outp.WriteLine("</tr>");
                }
                outp.WriteLine("</table>");
            }
        }
    }
}
