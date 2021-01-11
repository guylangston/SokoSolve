using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextRenderZ.Reporting
{
    public class MapToReportingRendererText :  IMapToReportingRenderer
    {
        public void Render<T>(IMapToReporting<T> mapping, IEnumerable<T> items, ITextWriterAdapter outp)
        {
            var columns = mapping.Columns;
            
            var table   = GenerateTable<T>(mapping, items);
            if (table == null)
            {
                outp.WriteLine("No Data.");
                return;
            }
            
            var maxSize = GetMax<T>(mapping, table);

            if (mapping.Title != null)
            {
                outp.Write("|");
                for (var ii = 0; ii < columns.Count; ii++)
                {
                    outp.Write("=");
                    var col = columns[ii];
                    outp.Write("".PadRight(maxSize[ii], '='));
                    outp.Write("=|");
                }
                outp.WriteLine();

                var width = maxSize.Sum() + columns.Count * 3 - 2;
                
                outp.Write("| ");
                var t = $" >>> {mapping.Title} <<<";
                outp.Write(t.PadLeft((width - t.Length/2)/2).PadRight(width-1));
                outp.Write(" |");
                outp.WriteLine();

                if (mapping.ByLine != null)
                {
                    outp.Write("| ");
                    t = $" >>> {mapping.ByLine} <<<";
                    outp.Write(t.PadRight(width));
                    outp.Write(" |");
                    outp.WriteLine();
                }
                
            }
            
            // Header
            outp.Write("| ");
            for (var ii = 0; ii < columns.Count; ii++)
            {
                var col = columns[ii];
                outp.Write(col.Title?.PadRight(maxSize[ii]));
                outp.Write(" | ");
            }
            outp.WriteLine(null);
            
            outp.Write("|");
            for (var ii = 0; ii < columns.Count; ii++)
            {
                outp.Write("-");
                var col = columns[ii];
                outp.Write("".PadRight(maxSize[ii], '-'));
                outp.Write("-|");
            }
            outp.WriteLine();
            
            // Body
            for (int yy = 0; yy < table.GetLength(1); yy++)
            {
                outp.Write("|");
                for (int xx = 0; xx < table.GetLength(0); xx++)
                {
                    outp.Write(" ");
                    var col  = columns[xx];
                    var cell = table[xx, yy]?.GetValueString();
                    if (cell != null)
                    {
                        if (col.TextAlign == TextAlign.Right) outp.Write(cell.PadLeft(maxSize[xx]));
                        else if (col.TextAlign == TextAlign.Center) outp.Write(cell.PadRight(maxSize[xx]/2).PadLeft(maxSize[xx]/2));
                        else outp.Write(cell.PadRight(maxSize[xx]));
                    }
                    else
                    {
                        outp.Write("".PadLeft(maxSize[xx]));
                    }
                    outp.Write(" |");
                }
                outp.WriteLine();
            }
        }
        
        private int[] GetMax<T>(IMapToReporting<T> mapping, Cell[,] table)
        {
            var   columns = mapping.Columns;
            int[] result  = new int[table.GetLength(0)];
            for (int xx = 0; xx < table.GetLength(0); xx++)
            {
                var max = columns[xx]?.Title?.Length ?? 0;
                for (int yy = 0; yy < table.GetLength(1); yy++)
                {
                    var v = table[xx, yy]?.GetValueString();
                    if (v != null && v.Length > max) max = v.Length;
                }

                result[xx] = max;
            }
            return result;

        }

        private Cell[,]? GenerateTable<T>(IMapToReporting<T> mapping, IEnumerable<T> items) => GenerateTable<T>(mapping.GetRows(items).Select(x=>x.ToList()).ToList());
        private Cell[,]? GenerateTable<T>(List<List<Cell>> byList)
        {
            if (byList == null || byList.Count == 0) return null;
            
            var maxCell = byList.Max(x => x.Count);
            var table   = new Cell[maxCell, byList.Count];
            for (int yy = 0; yy < byList.Count; yy++)
            {
                var row = byList[yy];
                for (int xx = 0; xx < row.Count; xx++)
                {
                    table[xx, yy] = row[xx];
                }
            }
            return table;

        }

    }
}