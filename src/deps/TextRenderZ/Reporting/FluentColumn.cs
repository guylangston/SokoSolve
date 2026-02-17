using System;
using TextRenderZ.Reporting.Adapters;

namespace TextRenderZ.Reporting
{
    public class FluentColumn<T>
    {
        public FluentColumn(ColumnInfo columnInfo)
        {
            ColumnInfo = columnInfo;
        }

        public ColumnInfo ColumnInfo { get; set; }

        public FluentColumn<T> Add(ICellAdapter adapter)
        {
            ColumnInfo.Add(adapter);
            return this;
        }

        public FluentColumn<T> Link(CellLink<T> link)
        {
            ColumnInfo.Add(link);
            return this;
        }

        public FluentColumn<T> Link(Func<Cell, T, string> getUrl) => Link(new CellLink<T>(getUrl));
    }
}
