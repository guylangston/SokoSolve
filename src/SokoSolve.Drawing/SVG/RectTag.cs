using System.Numerics;
using VectorInt;

namespace SokoSolve.Drawing.SVG
{
    public class RectTag : Tag
    {
        public RectTag() : base("rect")
        {
            
        }

        public RectTag(Rect2 r) : this()
        {
            TopLeft(r.TL);
            Size(r.Size);
        }

        public RectTag TopLeft(Vector2 tl)
        {
            SetAttr("x", tl.X);
            SetAttr("y", tl.Y);
            return this;
        }
        
        public RectTag Size(Vector2 s)
        {
            SetAttr("width", s.X);
            SetAttr("height", s.Y);
            return this;
        }

        public RectTag Style(StyleTag s)
        {
            SetAttr("style", s);
            return this;
        }
        public RectTag Style(string s)
        {
            SetAttr("style", s);
            return this;
        }
    }
}