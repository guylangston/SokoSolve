using System.IO;

namespace TextRenderZ.Reporting
{
    public class MapToReportingRendererSingleDt : IMapToReportingRendererSingle
    {
        public string         ContainerClass { get; set; } 
        public ICellFormatter CellFormatter  { get; set; }

        

        public MapToReportingRendererSingleDt(ICellFormatter cellFormatter)
        {
            CellFormatter = cellFormatter;
        }
        
        public MapToReportingRendererSingleDt() : this(new CellFormatter()) { }

        public void Render<T>(IMapToReporting<T> mapping, T item, ITextWriterAdapter outp)
        {
       
            outp.WriteLine($"<dl class='{ContainerClass}'>");
            
            
            foreach (var singleRow in mapping.GetRows(new []{item}))
            {

                foreach (var cell in singleRow)
                {
                    outp.Write("<dt>");
                    outp.Write(cell.Column.Title);
                    outp.WriteLine("</dt>");    
                    
                    var tag = new CellContainerTag("dd", null, null);
                    CellFormatter.WriteCell(outp, cell, ref tag);
                }
                

                
            }
            outp.WriteLine("</dl>");
        }
    }
}