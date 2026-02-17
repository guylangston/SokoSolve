using System;

namespace TextRenderZ.Reporting.Adapters
{
    public class CellLink
    {
        public static CellLink<T> Create<T>(Func<Cell, T, string> getUrl) => new CellLink<T>(getUrl);

        public static CellLink<T> Create<T>(Func<T, string> getUrl) => new CellLink<T>((cell, item) => getUrl(item));

    }

    public class CellLink<T> : ICellAdapter
    {
        private Func<Cell, T, string> getUrl;
        public  string                UrlClass { get; set; }

        public CellLink(Func<Cell, T, string> getUrl)
        {
            this.getUrl = getUrl;
        }

        public void Adapt(Cell cell)
        {
            cell.Info.Url      = getUrl(cell, (T)cell.ValueContainer);
            cell.Info.UrlClass = UrlClass;
        }

    }

}
