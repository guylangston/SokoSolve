using System.Collections.Generic;

namespace TextRenderZ.Reporting
{
    public class CellInfo
    {
        public string Id        { get; set; }
        public string ClassAttr { get; set; }

        public NumberStyle NumberStyle { get; set; }
        public bool IsNumber => NumberStyle != NumberStyle.NotNumber && NumberStyle != NumberStyle.Unknown;
        public bool        IsErr    { get; set; } // Number NaN, etc (not exception info)
        public bool        IsNeg    { get; set; } // Number NaN, etc (not exception info)
        public string      Prefix   { get; set; } // May be overridden per cell
        public string      Suffix   { get; set; } // May be overridden per cell
        public string      ToolTip  { get; set; }
        public string      Url      { get; set; }
        public string      UrlClass { get; set; }

        public void AddClass(string classIdent)
        {
            classIdent = classIdent.ToLowerInvariant();
            if (ClassAttr != null && ClassAttr.Contains(classIdent)) return;
            ClassAttr = ClassAttr == null
                ? classIdent
                : ClassAttr + " " + classIdent;
        }

        public Dictionary<string, string> Attributes { get; set; }

        public void AddAttr(string name, string val)
        {
            Attributes       ??= new Dictionary<string, string>();
            Attributes[name] =   val;
        }
    }
}
