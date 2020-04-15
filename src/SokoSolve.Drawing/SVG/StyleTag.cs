using System.Text;

namespace SokoSolve.Drawing.SVG
{
    public class StyleTag : Tag
    {
        public StyleTag() : base("style")
        {
        }

        public StyleTag Fill(string color)
        {
            SetAttr("fill", color);
            return this;
        }
        
        public StyleTag Stroke(float width, string color)
        {
            SetAttr("stroke-width", width);
            SetAttr("stroke", color);
            return this;
        }
        
        public override string ToString()
        {
            var sb = new StringBuilder();
            
            
            foreach (var pair in attr)
            {
                sb.Append(pair.Key);
                sb.Append(":");
                sb.Append(pair.Value);
                sb.Append(";");
            }
            
            return sb.ToString();
        }
    }
}