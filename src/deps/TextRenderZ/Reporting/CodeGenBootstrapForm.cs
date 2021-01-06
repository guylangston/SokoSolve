using System.IO;

namespace TextRenderZ.Reporting
{
    public class CodeGenBootstrapForm<T> : IMapToReportingCodeGen<T>
    {
        public CodeGenOutput Format => CodeGenOutput.Html;

        public void CodeGen(TextWriter output, IMapToReporting<T> report)
        {
            output.WriteLine("<form role=\"form\" method=\"post\" action=\"#\">");
            foreach (var col in report.Columns)
            {
                var prop = GetPropName(col);
                if (col.IsNumber)
                {
                    output.WriteLine(
                        $@"<div class=""form-group"">
    <label for=""ele-{prop}"">{col.Title}</label>
    <input type=""number"" class=""form-control"" id=""ele-{prop}"" name=""{prop}"" placeholder=""{col.Description}"">
  </div>");
                }
                else
                {
                    
                    output.WriteLine($@"<div class=""form-group"">
    <label for=""ele-{prop}"">{col.Title}</label>
    <input type=""text"" class=""form-control"" id=""ele-{prop}"" name=""{prop}"" placeholder=""{col.Description}"">
  </div>");
                    
                }

            }
            output.WriteLine("</form>");
        }

        // TODO: Move onto ColumnInfo as virtual
        private string GetPropName(ColumnInfo col)
        {
            if (col is ColumnInfoPropertyInfo prop) return prop.PropertyInfo.Name;
            return col.Title.Replace(" ", "");
        }
    }
}