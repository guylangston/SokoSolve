using System.Collections.Generic;
using System.Text;

namespace TextRenderZ.Reporting
{
    public class MapToReportingRendererCsv : IMapToReportingRenderer
    {
        
         
        /// <summary>
        /// Turn a string into a CSV cell output
        /// http://stackoverflow.com/questions/6377454/escaping-tricky-string-to-csv-format
        /// </summary>
        /// <param name="str">String to output</param>
        /// <returns>The CSV cell formatted string</returns>
        public static string EncodeStringForCSV(string str)
        {
            if (str == null) return null;
            str = str.Replace("\n", "<br/>");
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }
        
        public void Render<T>(IMapToReporting<T> mapping, IEnumerable<T> items, ITextWriterAdapter outp)
        {
            var columns = mapping.Columns;
            var c       = 0;
            foreach (var column in columns)
            {
                if (c > 0) outp.Write(",");
                outp.Write(column.Title);
                c++;
            }
            outp.WriteLine();

            foreach (var item in items)
            {
                c = 0;
                foreach (var column in columns)
                {
                    if (c > 0) outp.Write(",");
                    var v = column.GetCellValue(item);
                    outp.Write(EncodeStringForCSV(v?.ToString() ?? string.Empty));
                    c++;
                }
                outp.WriteLine();
            }
        }
    }
}