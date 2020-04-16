using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SokoSolve.Core.Reporting
{
    public static class MapToReporting
    {
        public static MapToReporting<T>  Create<T>() => new MapToReporting<T>();
    }
    
    public class MapToReporting<T> : IMapToReporting<T>
    {
        List<PropToColumn> columns = new List<PropToColumn>();
        private PropertyInfo[] props;

        public MapToReporting()
        {
            this.props = typeof(T).GetProperties();
        }

        public MapToReporting<T> AddColumn<TP>(Expression<Func<T, TP>> exp)
        {
            throw new NotImplementedException();
            return this;
        }

        
        public MapToReporting<T> AddColumn<TP>(string title, Func<T, TP> getVal)
        {
            var p = new PropToColumn(typeof(TP), title);
            p.GetCell = o => CellFactory(p, o);
            columns.Add(p);
            
            Cell CellFactory(PropToColumn propToColumn, object o) =>
                AdaptForDisplay(new Cell()
                {
                    Style = propToColumn,
                    Value = getVal((T)o)
                });
            return this;
        }
        
        public MapToReporting<T> AddColumn(string propName) => AddColumn(props.First(x => x.Name == propName));
        public MapToReporting<T> AddColumn(PropertyInfo info)
        {
            var p = new PropToColumn(info.PropertyType, info.Name)
            {
                PropertyInfo = info
            };
            p.GetCell = o => CellFactory(p, o);
            columns.Add(p);
            
            Cell CellFactory(PropToColumn propToColumn, object o) =>
                AdaptForDisplay(new Cell()
                {
                    Style = propToColumn,
                    Value = propToColumn.PropertyInfo.GetValue(o)
                });
            return this;
        }
        

        private Cell AdaptForDisplay(Cell cell)
        {
            cell.ValueDisplay = cell.Value;
            if (cell.Style is PropToColumn col)
            {
                if (col.StringFormat != null)
                {
                    cell.ValueDisplay = string.Format("{0:" + col.StringFormat + "}", cell.Value); // TODO: Messy, there must be a cleaner way...
                }
            }
            return cell;
        }

        class PropToColumn : CellStyle
        {
            public PropToColumn(Type valueType, string title)
            {
                base.Title = title;
                ValueType = valueType;

                if (ValueType == typeof(double) || ValueType == typeof(decimal) || ValueType == typeof(float))
                {
                    TextAlign    = TextAlign.Right;
                    StringFormat = "#,##0.00";
                }
                else if (ValueType == typeof(int) || ValueType == typeof(long))
                {
                    TextAlign    = TextAlign.Right;
                    StringFormat = "#,##0";
                }
            }

            public Type ValueType { get;  }
            public Func<object, Cell> GetCell { get; set; }
            
            public PropertyInfo? PropertyInfo { get; set; }
            public string?  StringFormat { get; set; }
        }

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

            private Cell GetCell(PropToColumn col, T data)
            {
                try
                {
                    return col.GetCell(data);
                }
                catch (Exception e)
                {
                    return new Cell()
                    {
                        Style = col,
                        Error = e,
                        Value = "!ERR!"
                    };
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        public IReadOnlyList<CellStyle> Columns => columns;
        public IEnumerable<IMapToRow<T>> GetRows(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return new MapToRow(this, item);
            }
        }

        public IMapToReporting<T> RenderTo(IEnumerable<T> items, IMapToReportingRenderer renderer, TextWriter outp )
        {
            renderer.Render(this, items, outp);

            return this;
        }

       
        public IMapToReporting<T> RenderTo(IEnumerable<T> items, IMapToReportingRenderer renderer, StringBuilder sb)
        {
            using var sw = new StringWriter(sb);
            RenderTo(items, renderer, sw);
            return this;
        }

        public void CodeGen(TextWriter output)
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                output.WriteLine($"\t.AddColumn(\"{prop.Name}\", x=>x.{prop.Name})");
            }
        }
    }
}