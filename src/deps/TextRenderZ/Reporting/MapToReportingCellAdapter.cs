using System;

namespace TextRenderZ.Reporting
{
    public class MapToReportingCellAdapter : IMapToReportingCellAdapter
    {

        public virtual void Enrich(ColumnInfo col)
        {
            if (col.TargetType == typeof(int) 
             
                || col.TargetType == typeof(decimal)
                || col.TargetType == typeof(double)
                || col.TargetType == typeof(float)
                || col.TargetType == typeof(byte)
                || col.TargetType == typeof(long)
                || col.TargetType == typeof(uint)
                || col.TargetType == typeof(ulong)
                )
            {
                col.TextAlign = TextAlign.Right;
                col.NumberStyle = NumberStyle.Number;
                return;
            }
        }
        
        public virtual Cell ConvertToCell(ColumnInfo col, object value, object container)
        {
            if (value is Cell c) return c; // Passthrough 
            var cell = new Cell(col, null, value, container);
            cell.IsNull = value == null;
            Enrich(cell);

            return cell;
        }

        public virtual void Enrich(Cell cell)
        {
            if (cell.ValueInput is int valueInt)
            {
                cell.IsNull = valueInt == Decimal.MinValue;
                
                cell.CellInfo       ??= new CellInfo();
                if (cell.Column.NumberStyle == NumberStyle.Unknown) cell.CellInfo.NumberStyle = NumberStyle.Number;
                cell.CellInfo.IsErr = valueInt == Decimal.MaxValue;
                cell.CellInfo.IsNeg = valueInt < 0;
                
                cell.ValueDisplay = valueInt.ToString("#,##0");
                return;
            }

            if (cell.ValueInput is DateTime valueDate)
            {
                cell.IsNull = valueDate == DateTime.MinValue;
                
                cell.CellInfo       ??= new CellInfo();
                cell.CellInfo.IsErr = valueDate == DateTime.MaxValue;
                
                
                cell.ValueDisplay = valueDate.ToString("yyyy-MM-dd");
                return;
            }
            
            if (cell.ValueInput is double valueDouble)
            {
                cell.IsNull = double.IsNaN(valueDouble);
                
                cell.CellInfo       ??= new CellInfo();
                if (cell.Column.NumberStyle == NumberStyle.Unknown) cell.CellInfo.NumberStyle = NumberStyle.Number;
                cell.CellInfo.ToolTip = valueDouble.ToString();
                cell.CellInfo.IsErr =   double.MaxValue.Equals(valueDouble);
                cell.CellInfo.IsNeg =   valueDouble < 0;
                
                cell.ValueDisplay = valueDouble.ToString("#,##0.00");
                return;
            }
            
            if (cell.ValueInput is decimal valDecimal)
            {
                cell.IsNull = valDecimal == Decimal.MinValue;
                
                cell.CellInfo       ??= new CellInfo();
                if (cell.Column.NumberStyle == NumberStyle.Unknown) cell.CellInfo.NumberStyle = NumberStyle.Number;
                cell.CellInfo.ToolTip = valDecimal.ToString();
                cell.CellInfo.IsErr = valDecimal == Decimal.MaxValue;
                cell.CellInfo.IsNeg =   valDecimal < 0;
                
                cell.ValueDisplay = valDecimal.ToString("#,##0.00");
                return;
            }
            
            if (cell.ValueInput is float valFloat)
            {
                cell.IsNull = float.IsNaN(valFloat);
                
                cell.CellInfo       ??= new CellInfo();
                if (cell.Column.NumberStyle == NumberStyle.Unknown) cell.CellInfo.NumberStyle = NumberStyle.Number;
                cell.CellInfo.ToolTip = valFloat.ToString();
                cell.CellInfo.IsErr =   float.MaxValue.Equals(valFloat);
                cell.CellInfo.IsNeg =   valFloat < 0;
                
                cell.ValueDisplay = valFloat.ToString("#,##0.00");
                return;
            }
            
            if (cell.ValueInput is long valLong)
            {
                cell.IsNull = valLong == long.MinValue;
                
                cell.CellInfo       ??= new CellInfo();
                if (cell.Column.NumberStyle == NumberStyle.Unknown) cell.CellInfo.NumberStyle = NumberStyle.Number;
                cell.CellInfo.ToolTip = valLong.ToString();
                cell.CellInfo.IsErr =   valLong == long.MaxValue;
                cell.CellInfo.IsNeg =   valLong < 0;
                
                cell.ValueDisplay = valLong.ToString("#,##0");
                return;
            }


            cell.ValueDisplay = cell.ValueInput;
        }


        public Cell ConvertToCell(ColumnInfo col, Exception error, object container)
        {
            var cell = new Cell(col, null, null, container);
            cell.Error = error;
            cell.IsNull = true;
            return cell;
        }
    }
}