using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace TextRenderZ.Reporting
{
    public static class MapToReporting
    {
        public static MapToReporting<T> Create<T>(IEnumerable<T> _) => new MapToReporting<T>();
        public static MapToReporting<T> Create<T>(IEnumerable<T> _, IMapToReportingCellAdapter adapter) => new MapToReporting<T>(adapter);
        public static MapToReporting<T>  Create<T>() => new MapToReporting<T>();
        
        public static MapToReporting<T> Create<T>(IMapToReportingCellAdapter adapter) => new MapToReporting<T>(adapter);
        public static MapToReporting<T> Create<T>(IMapToReportingCellAdapter adapter, IEnumerable<T> _) => new MapToReporting<T>(adapter);
    }

    public interface IMapToReportingCellAdapter
    {
        void Enrich(ColumnInfo col);
        void Enrich(Cell cell);
        Cell ConvertToCell(ColumnInfo col, object value, object container);
        Cell ConvertToCell(ColumnInfo col, Exception error, object container);
    }

    public enum CodeGenOutput
    {
        Text,
        CSharp,
        Html,
        Sql
    }

    public interface IMapToReportingCodeGen<T>
    {
        CodeGenOutput Format { get;  }
        void CodeGen(TextWriter output, IMapToReporting<T> report);
    }

    public static class MapToReportingExt
    {
        public static IMapToReporting<T> RenderTo<T>(this IMapToReporting<T> map, IEnumerable<T> items, IMapToReportingRenderer renderer, StringBuilder sb) 
        {
            using var sw = new StringWriter(sb);
            map.RenderTo(items, renderer, new TextWriterAdapter(sw));
            return map;
        }
        
        public static IMapToReporting<T> RenderTo<T>(this IMapToReporting<T> map, IEnumerable<T> items, IMapToReportingRenderer renderer, TextWriter tw) 
        {
            map.RenderTo(items, renderer, new TextWriterAdapter(tw));
            return map;
        }
        
        public static IMapToReporting<T> RenderTo<T>(this IMapToReporting<T> map, T item, IMapToReportingRendererSingle renderer, StringBuilder sb) 
        {
            using var sw = new StringWriter(sb);
            map.RenderTo(item, renderer, new TextWriterAdapter(sw));
            return map;
        }
        
        public static IMapToReporting<T> RenderTo<T>(this IMapToReporting<T> map, T item, IMapToReportingRendererSingle renderer, TextWriter tw) 
        {
            map.RenderTo(item, renderer, new TextWriterAdapter(tw));
            return map;
        }
    
    }
    

    public class MapToReporting<T> : IMapToReporting<T>
    {
        private readonly List<ColumnInfo> columns = new List<ColumnInfo>();
        private PropertyInfo[]? _props;
        
        private PropertyInfo[] props => _props ??= typeof(T).GetProperties();
        
        
        public MapToReporting(IMapToReportingCellAdapter cellAdapter)
        {
            CellAdapter = cellAdapter;
        }

        public MapToReporting() : this(new MapToReportingCellAdapter()) { }

        public IMapToReportingCellAdapter CellAdapter { get; set; }

        private void Add(ColumnInfo c)
        {
            CellAdapter.Enrich(c);
            columns.Add(c);
        }
        
        public MapToReporting<T> AddColumn(ColumnInfo manual)
        {
            Add(manual);
            return this;
        }
        public MapToReporting<T> AddColumns(IEnumerable<ColumnInfo> cols)
        {
            foreach (var col in cols)
            {
                CellAdapter.Enrich(col);
            }
            columns.AddRange(cols);
            return this;
        }
        
        public MapToReporting<T> AddColumns()
        {
            foreach (var propertyInfo in props)
            {
                AddColumn(StringUtil.UnCamel(propertyInfo.Name), propertyInfo);
            }
            return this;
        }

        public MapToReporting<T> AddColumn<TP>(Expression<Func<T, TP>> exp)
        {
            throw new NotImplementedException();
        }

        public MapToReporting<T> AddColumn<TP>(string title, Func<T, TP> getVal, Action<FluentColumn<T>>? setupCol = null)
        {
#pragma warning disable 8605
            var columnInfoFunc = new ColumnInfoFunc(typeof(TP), typeof(T), title, o => (object?)getVal((T) o));
#pragma warning restore 8605
            if (setupCol != null) setupCol(new FluentColumn<T>(columnInfoFunc));
            Add(columnInfoFunc);
            return this;
        }
        
        
        public MapToReporting<T> AddColumn(string? title, PropertyInfo info, Action<FluentColumn<T>>? setupCol = null)
        {
            var columnInfoPropertyInfo = new ColumnInfoPropertyInfo(info, typeof(T), title ?? info.Name);
            Add(columnInfoPropertyInfo);
            if (setupCol != null) setupCol(new FluentColumn<T>(columnInfoPropertyInfo));
            return this;
        }
        
        public MapToReporting<T> AddColumn(string propName) => AddColumn(null, props.First(x => x.Name == propName), null);



        class MapToRow : IMapToRow<T>
        {
            private readonly MapToReporting<T> owner;
            private readonly T item;

            public MapToRow(MapToReporting<T> owner, T item)
            {
                this.owner = owner;
                this.item = item;
            }

            public IEnumerator<Cell> GetEnumerator()
            {
                foreach (var col in owner.columns)
                {
                    var c = GetCell(col, item);
                    yield return c;
                }
            }

            private Cell GetCell(ColumnInfo col, T data)
            {
                try
                {
                    var obj = col.GetCellValue(data);
                    var c =  owner.CellAdapter.ConvertToCell(col, obj, data);
                    if (col.Adapters != null) 
                    {
                        foreach (var adapter in col.Adapters)
                        {
                            adapter.Adapt(c);
                        }    
                    }

                    return c;
                }
                catch (Exception e)
                {
                    return owner.CellAdapter.ConvertToCell(col, e, data);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        public IReadOnlyList<ColumnInfo> Columns => columns;
        public IEnumerable<IMapToRow<T>> GetRows(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return new MapToRow(this, item);
            }
        }
        
        public IMapToReporting<T> RenderTo(T item, IMapToReportingRendererSingle renderer, ITextWriterAdapter outp )
        {
            renderer.Render(this, item, outp);
            return this;
        }

        
        public IMapToReporting<T> RenderTo(IEnumerable<T> items, IMapToReportingRenderer renderer, ITextWriterAdapter outp )
        {
            renderer.Render(this, items, outp);

            return this;
        }

       
       

        public void CodeGen(TextWriter output, IMapToReportingCodeGen<T> codeGen = null, bool wrapHtml = true)
        {
            codeGen ??= new CodeGenAddCols();

            if (!wrapHtml)
            {
                codeGen.CodeGen(output, this);
                return;
            }

            if (codeGen.Format == CodeGenOutput.CSharp)
            {
                output.WriteLine("<pre class='code-cs'><code>");
                codeGen.CodeGen(output, this);
                output.WriteLine("</code></pre>");
                return;
            }
            
            if (codeGen.Format == CodeGenOutput.Html)
            {
                output.WriteLine("<textarea class=\"w-100\" style='width: 100%;'>");
                codeGen.CodeGen(output, this);
                output.WriteLine("</textarea>");
                return;
            }
            
            
            output.WriteLine($"<pre class='code-{codeGen.Format}'><code>");
            codeGen.CodeGen(output, this);
            output.WriteLine("</code></pre>");
            
        }

        public class CodeGenAddCols : IMapToReportingCodeGen<T>
        {
            public CodeGenOutput Format => CodeGenOutput.CSharp;

            public void CodeGen(TextWriter output, IMapToReporting<T> report)
            {
                foreach (var prop in typeof(T).GetProperties())
                {
                    output.WriteLine($"\t.AddColumn(\"{prop.Name}\", x=>x.{prop.Name})");
                }
            }
        }
    }
    
     
}