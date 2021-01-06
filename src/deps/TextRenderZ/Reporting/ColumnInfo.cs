using System;
using System.Collections.Generic;

namespace TextRenderZ.Reporting
{
    public abstract class ColumnInfo
    {
        public ColumnInfo(Type targetType, Type containerType, string title)
        {
            TargetType    = targetType;
            ContainerType = containerType;
            Title         = title;
        }

        public Type TargetType    { get;  }
        public Type ContainerType { get; }

        public abstract string PropName { get; }
        public string Title    { get; }
        
        public string?     Description { get; set; }
        public TextAlign   TextAlign   { get; set; }
        public NumberStyle NumberStyle    { get; set; }
        public bool IsNumber => NumberStyle != NumberStyle.NotNumber && NumberStyle != NumberStyle.Unknown;
        public string      Prefix      { get; set; } // May be overridden per cell
        public string      Suffix      { get; set; } // May be overridden per cell
        public string      GroupTitle { get; set; }

        
        
        
        public IReadOnlyDictionary<string, string> Attributes { get; set; }
        
        public List<ICellAdapter> Adapters { get; set; }

        public ColumnInfo Add(ICellAdapter adapter)
        {
            Adapters ??= new List<ICellAdapter>();
            Adapters.Add(adapter);
            return this;
        }

        public abstract object GetCellValue(object? container);

        public ColumnInfo AsPercentage()
        {
            Suffix   = "%";
            NumberStyle = NumberStyle.Percentage;
            return this;
        }
    }

    
    
    
}