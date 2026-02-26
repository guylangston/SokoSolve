using VectorInt;

namespace SokoSolve.Drawing.SVG
{
    public class BoxAndTextGroup : GroupTag
    {
        public BoxAndTextGroup(Rect2 box, string text, StyleTag style = null)
        {
            Add(this.Background = new RectTag(box));
            Add(this.Text = new TextTag(box, text));
            if (style != null) Background.Style(style);
        }

        public TextTag Text { get; set; }

        public RectTag Background { get; set; }
    }

}
