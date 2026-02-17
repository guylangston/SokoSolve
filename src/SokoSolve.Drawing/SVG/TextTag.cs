using System.Numerics;
using VectorInt;

namespace SokoSolve.Drawing.SVG
{
    public class TextTag : Tag
    {

        public TextTag(Rect2 r, string txt) : base("text")
        {
            Inner = txt;
            SetAttr("dominant-baseline", "middle");
            SetAttr("text-anchor", "middle");
            Style(new StyleTag().Fill("black"));
            Center(r.C);
            Size(r.Size);
        }

        public TextTag Center(Vector2 tl)
        {
            SetAttr("x", tl.X);
            SetAttr("y", tl.Y);
            return this;
        }

        public TextTag Size(Vector2 s)
        {
            SetAttr("width", s.X);
            SetAttr("height", s.Y);
            return this;
        }

        public TextTag Style(StyleTag s)
        {
            SetAttr("style", s);
            return this;
        }
    }
}
