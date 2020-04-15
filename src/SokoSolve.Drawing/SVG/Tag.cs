using System.Collections.Generic;
using System.Text;

namespace SokoSolve.Drawing.SVG
{
    public class Tag 
    {
        public Tag(string name)
        {
            Name = name;
        }

        protected Dictionary<string, object> attr = new Dictionary<string, object>();
        
        public string Name { get; set; }

        public Tag SetAttr<T>(string name, T val)
        {
            attr[name] = val;
            return this;
        }

        public object Inner { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("<");
            sb.Append(Name);
            sb.Append(" ");
            foreach (var pair in attr)
            {
                sb.Append(pair.Key);
                sb.Append("='");
                sb.Append(pair.Value);
                sb.Append("' ");
            }

            if (Inner != null)
            {
                sb.AppendLine(">");

                sb.Append(Inner);
                
                sb.Append("</");
                sb.Append(Name);
                sb.AppendLine(">");
                
            }
            else
            {
                sb.AppendLine("/>");
            }
            
            
            return sb.ToString();
        }
    }
}