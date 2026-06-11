using System;
using System.Collections.Generic;

namespace TextRenderZ.Reporting.Adapters
{
    public class Dim<T>
    {
#pragma warning disable CS8618
        public string Name  { get; set; }
        public int    Index { get; set; }
        public T      Value { get; set; }
#pragma warning restore CS8618
    }

    public class TableReportModel<TCol, TRow>
    {
        public List<Dim<TCol>> Columns { get;  } = new List<Dim<TCol>>();
        public List<Dim<TRow>> Rows { get;  } = new List<Dim<TRow>>();

        public void AddCols(IEnumerable<TCol> cols, Func<TCol, string> getName)
        {
            foreach (var col in cols)
            {
                var cc = new Dim<TCol>()
                {
                    Index = Columns.Count,
                    Name  = getName(col),
                    Value = col
                };
                Columns.Add(cc);
            }
        }

        public void AddRows(IEnumerable<TRow> rows, Func<TRow, string> getName)
        {
            foreach (var row in rows)
            {
                var cc = new Dim<TRow>()
                {
                    Index = Rows.Count,
                    Name  = getName(row),
                    Value = row
                };
                Rows.Add(cc);
            }
        }

    }
}
