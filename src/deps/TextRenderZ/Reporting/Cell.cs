using System;

namespace TextRenderZ.Reporting
{
    
    
    
    public sealed class Cell
    {
        public Cell(ColumnInfo colInfo, CellInfo? cellInfo, object? valueInput, object containerValue)
        {
            Column         = colInfo;
            CellInfo       = cellInfo;
            ValueInput     = valueInput;
            ValueDisplay = valueInput;
            ValueContainer = containerValue;
        }

        public ColumnInfo Column   { get;  }
        public CellInfo?  CellInfo { get; set; }

        public object? ValueContainer { get; set; }
        public object? ValueInput     { get; set; }
        public object? ValueDisplay   { get; set; }
        
        public Exception? Error  { get; set; }
        public bool       IsNull { get; set; }

        public          object? GetValue()       => ValueDisplay ?? ValueInput;  
        public          string? GetValueString() => GetValue()?.ToString();
        public override string  ToString()       => GetValueString();

        public CellInfo Info
        {
            get
            {
                if (CellInfo == null) CellInfo = new CellInfo();
                return CellInfo;
            }
        }
    }
}