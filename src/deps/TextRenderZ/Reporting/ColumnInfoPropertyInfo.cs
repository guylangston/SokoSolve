using System;
using System.Reflection;

namespace TextRenderZ.Reporting
{
    public class ColumnInfoPropertyInfo : ColumnInfo
    {
        public ColumnInfoPropertyInfo(PropertyInfo info, Type containerType, string title)
            : base(info.PropertyType, containerType, title)
        {
            PropertyInfo = info;

        }

        public PropertyInfo PropertyInfo { get; }

        public override string PropName => PropertyInfo.Name;
        public override object GetCellValue(object container) => PropertyInfo.GetValue(container);
    }
}
