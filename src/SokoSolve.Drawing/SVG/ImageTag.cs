using System.Numerics;
using VectorInt;

namespace SokoSolve.Drawing.SVG
{
    public class ImageTag : Tag
    {
        public ImageTag() : base("image")
        {
            
        }

        public ImageTag(Rect2 r, string href, string? title=null) : this()
        {
            SetAttr("href", href);
            if (title != null)
            {
                Inner = $"<title>{title}</title>";
            }
            TopLeft(r.TL);
            Size(r.Size);
        }

        public ImageTag TopLeft(Vector2 tl)
        {
            SetAttr("x", tl.X);
            SetAttr("y", tl.Y);
            return this;
        }
        
        public ImageTag Size(Vector2 s)
        {
            SetAttr("width", s.X);
            SetAttr("height", s.Y);
            return this;
        }

        public ImageTag Style(StyleTag s)
        {
            SetAttr("style", s);
            return this;
        }
    }
}