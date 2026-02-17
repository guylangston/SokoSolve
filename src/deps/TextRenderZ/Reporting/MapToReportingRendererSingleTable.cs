using System.IO;

namespace TextRenderZ.Reporting
{
    public class MapToReportingRendererSingleTable : IMapToReportingRendererSingle
    {
        public string         ContainerClass { get; set; } = "table table-sm table-form";
        public ICellFormatter CellFormatter  { get; set; }

        public MapToReportingRendererSingleTable(ICellFormatter cellFormatter)
        {
            CellFormatter = cellFormatter;
        }

        public MapToReportingRendererSingleTable() : this(new CellFormatter()) { }

        public void Render<T>(IMapToReporting<T> mapping, T item, ITextWriterAdapter outp)
        {

            outp.WriteLine($"<table class='{ContainerClass}'>");

            foreach (var singleRow in mapping.GetRows(new []{item}))
            {

                foreach (var cell in singleRow)
                {
                    outp.Write("<tr><th>");
                    outp.Write(cell.Column.Title);
                    outp.WriteLine("</th>");

                    var tag = new CellContainerTag("td", null, null);
                    CellFormatter.WriteCell(outp, cell, ref tag);
                    outp.WriteLine("</tr>");
                }

            }
            outp.WriteLine("</table>");
        }
    }
}
