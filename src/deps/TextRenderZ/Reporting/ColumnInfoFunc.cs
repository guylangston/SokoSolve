using System;

namespace TextRenderZ.Reporting
{
    public class ColumnInfoFunc : ColumnInfo
    {
        private Func<object?, object?> func;
        
        public static ColumnInfoFunc Create<T, TP>(string title, Func<T, TP> getValue) 
            => new ColumnInfoFunc(typeof(T), typeof(TP), title, x => (object)getValue((T)x));

        public ColumnInfoFunc(Type targetType, Type containerType, string title, Func<object?, object?> getValue) 
            : base(targetType, containerType, title)
        {
            this.func = getValue;
        }

        public override string PropName => Title?.Replace(" ", "");
        public override object GetCellValue(object container) => func(container);
    }
}