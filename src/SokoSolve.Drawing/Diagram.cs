using System.Collections.Generic;
using VectorInt;

namespace SokoSolve.Drawing
{
    public class Diagram<TDomain>
    {
        public Rect2 Canvas  { get; set; }
        public Rect2 Surface { get; set; }
        public List<TDomain> Members { get; set; }
 
        public ITranslation<TDomain, Rect2> PlotTransform { get; set; }
    }
    
}