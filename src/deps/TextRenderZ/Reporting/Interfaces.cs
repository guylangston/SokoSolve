using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TextRenderZ.Reporting
{
    public interface ICellAdapter
    {
        void Adapt(Cell cell);
    }

    public class CellAdapter : ICellAdapter
    {
        private readonly Action<Cell> func;

        public CellAdapter(Action<Cell> func)
        {
            this.func = func;
        }

        public void Adapt(Cell cell) => func(cell);
    }
    
    public enum TextAlign { None, Left, Center, Right }
    public enum NumberStyle { Unknown, NotNumber, Number, Percentage, PercentageMul100, Currency }

    /// <summary>
    /// For ease the nomenclature of Column/Row/Cell; think of a single Item as always wrapped into Cell[1] array
    /// </summary>
    public interface IMapToReporting<T> 
    {
        public string Title { get; set; }
        public string ByLine { get; set; }
        
        public IReadOnlyList<ColumnInfo> Columns { get;  }
        public IEnumerable<IMapToRow<T>> GetRows(IEnumerable<T> items);
        
        public IMapToReporting<T> RenderTo(T item, IMapToReportingRendererSingle renderer, ITextWriterAdapter outp);
        public IMapToReporting<T> RenderTo(IEnumerable<T> items, IMapToReportingRenderer renderer, ITextWriterAdapter outp);
    }
    
    public interface ITextWriterAdapter
    {
        void Write(string? s);
        void WriteLine(string? s);
        void WriteLine();
    }

    public interface IMapToReportingRenderer
    {
        void Render<T>(IMapToReporting<T> mapping, IEnumerable<T> items, ITextWriterAdapter outp);
    }
    
    public interface IMapToReportingRendererSingle
    {
        void Render<T>(IMapToReporting<T> mapping, T items, ITextWriterAdapter outp);
    }
    
    public interface IMapToRow<T> : IEnumerable<Cell>
    {
        
    }
    
    
}
