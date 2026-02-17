using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Security.Cryptography;

namespace TextRenderZ.Reporting
{
    public struct CellContainerTag
    {
        public CellContainerTag(string tagName, string? id, string? @class)
        {
            TagName = tagName;
            Id = id;
            ClassAttr = @class;
            Attributes = null;
        }

        public string TagName { get; set; }
        public string? Id    { get; set; }
        public string? ClassAttr { get; set; }

        public void AddClass(string classIdent)
        {
            classIdent = classIdent.ToLowerInvariant();
            if (ClassAttr != null && ClassAttr.Contains(classIdent)) return;
            ClassAttr = ClassAttr == null
                ? classIdent
                : ClassAttr + " " + classIdent;
        }

        public Dictionary<string, string>? Attributes { get; set; }
    }

    public interface ICellFormatter
    {
        void WriteCell(ITextWriterAdapter tw, Cell input, ref CellContainerTag tag);
    }

    public class CellFormatter : ICellFormatter
    {
        public string NullToken { get; set; } = "~";
        public string ErrorToken { get; set; } = "#ERR#";

        public void WriteCell(ITextWriterAdapter tw, Cell inputValue, ref CellContainerTag tag)
        {
            MapToTag(inputValue, ref tag);

            tw.Write($"<{tag.TagName}");
            if (tag.Id != null)
            {
                tw.Write($" id='{tag.Id}'");
            }

            if (tag.ClassAttr != null)
            {
                tw.Write($" class='{tag.ClassAttr}'");
            }

            if (inputValue.CellInfo?.ToolTip != null)
            {
                tw.Write($" title='{inputValue.CellInfo.ToolTip}'");
            }

            if (inputValue.CellInfo?.Attributes != null)
            {
                foreach (var pair in inputValue.CellInfo?.Attributes )
                {
                    tw.Write($" {pair.Key}='{pair.Value}'");
                }
            }
            if (tag.Attributes != null)
            {
                foreach (var pair in tag.Attributes)
                {
                    tw.Write($" {pair.Key}='{pair.Value}'");
                }
            }
            tw.Write(">");
            if (inputValue.CellInfo?.Url != null)
            {
                tw.Write($"<a href='{inputValue.CellInfo?.Url}' class='{inputValue.CellInfo?.UrlClass}'>");
            }

            if (inputValue.Error != null)
            {
                tw.Write(ErrorToken);
            }
            else if (inputValue.IsNull)
            {
                tw.Write(NullToken);
            }
            else
            {
                var px = inputValue.CellInfo?.Prefix ?? inputValue.Column.Prefix;
                if (px != null)
                {
                    tw.Write("<span class='prefix'>");
                    tw.Write(px);
                    tw.Write("</span>");
                }

                tw.Write(inputValue.ValueDisplay?.ToString());

                var sx =  inputValue.CellInfo?.Suffix ?? inputValue.Column.Suffix;
                if (sx != null)
                {
                    tw.Write("<span class='suffix'>");
                    tw.Write(sx);
                    tw.Write("</span>");

                }
            }
            if (inputValue.CellInfo?.Url != null)
            {
                tw.Write($"</a>");
            }
            tw.WriteLine($"</{tag.TagName}>");
        }

        private void MapToTag(Cell inputValue, ref CellContainerTag tag)
        {
            if (inputValue.CellInfo != null)
            {
                // Do these first, the override later
                tag.Id        = inputValue.CellInfo.Id;
                tag.ClassAttr = inputValue.CellInfo.ClassAttr;
            }

            if (inputValue.Error != null || inputValue.CellInfo?.IsErr == true)
            {
                tag.AddClass("err");
                if (inputValue.Error != null)
                {
                    tag.Attributes ??= new Dictionary<string, string>();
                    tag.Attributes["data-error"] = StringUtil.Elipse(inputValue.Error.Message, 80);
                }
            }

            if (inputValue.Column.TextAlign != TextAlign.None) tag.AddClass($"align-{inputValue.Column.TextAlign.ToString().ToLowerInvariant()}");

            if (inputValue.IsNull) tag.AddClass("null");
            if (inputValue.Column.IsNumber) tag.AddClass("num");
            if (inputValue.Column.NumberStyle == NumberStyle.Percentage) tag.AddClass("num-pct");
            if (inputValue.CellInfo != null)
            {
                if (inputValue.CellInfo.IsNumber) tag.AddClass("num");
                if (inputValue.CellInfo.NumberStyle == NumberStyle.Percentage) tag.AddClass("num-pct");
                if (inputValue.CellInfo.IsErr)  tag.AddClass("err");
                if (inputValue.CellInfo.IsNeg)  tag.AddClass("num-neg");
            }
        }

    }
}
