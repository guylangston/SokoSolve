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
                    var c = col.GetCell(item);
                    yield return c;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }


        public IReadOnlyList<CellStyle> Columns { get; }
        public IEnumerable<IMapToRow<T>> GetRows(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                yield return new MapToRow(this, item);
            }
        }

        public IMapToReporting<T> RenderTo(TextWriter outp, IEnumerable<T> items)
        {
            var table = GenerateTable(items);
            var maxSize = GetMax(table);
            
            // Header
            for (var ii = 0; ii < columns.Count; ii++)
            {
                var col = columns[ii];
                outp.Write(col.Title.PadRight(maxSize[ii]));
                outp.Write(" | ");
            }
            outp.WriteLine();
            
            for (var ii = 0; ii < columns.Count; ii++)
            {
                var col = columns[ii];
                outp.Write("".PadRight(maxSize[ii], '='));
                outp.Write("=|=");
            }
            outp.WriteLine();
            
            // Body
            for (int yy = 0; yy < table.GetLength(1); yy++)
            {
                for (int xx = 0; xx < table.GetLength(0); xx++)
                {
                    var col = columns[xx];
                    if (col.TextAlign == TextAlign.Left) outp.Write(table[xx, yy].GetValueString().PadRight(maxSize[xx]));
                    else if (col.TextAlign == TextAlign.Right) outp.Write(table[xx, yy].GetValueString().PadLeft(maxSize[xx]));
                    else if (col.TextAlign == TextAlign.Center) outp.Write(table[xx, yy].GetValueString().PadRight(maxSize[xx]/2).PadLeft(maxSize[xx]/2));
                    
                    outp.Write(" | ");
                }
                outp.WriteLine();
            }

            return this;
        }

        private int[] GetMax(Cell[,] table)
        {
            int[] result = new int[table.GetLength(0)];
            for (int xx = 0; xx < table.GetLength(0); xx++)
            {
                var max = columns[xx].Title.Length;
                for (int yy = 0; yy < table.GetLength(1); yy++)
                {
                    var v = table[xx, yy].GetValueString();
                    if (v != null && v.Length > max) max = v.Length;
                }

                result[xx] = max;
            }
            

            return result;

        }

        private Cell[,] GenerateTable(IEnumerable<T> items) => GenerateTable(GetRows(items).Select(x=>x.ToList()).ToList());
        private Cell[,] GenerateTable(List<List<Cell>> byList)
        {
            var maxCell = byList.Max(x => x.Count);
            var table = new Cell[maxCell, byList.Count];
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

        public IMapToReporting<T> RenderTo(StringBuilder sb, IEnumerable<T> items)
        {
            using var sw = new StringWriter(sb);
            RenderTo(sw, items);
            return this;
        }
    }
}