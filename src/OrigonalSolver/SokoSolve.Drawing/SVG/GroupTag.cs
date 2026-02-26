using System;
using System.Collections.Generic;

namespace SokoSolve.Drawing.SVG
{
    public class GroupTag : Tag
    {
        private readonly List<Tag> inner;

        public GroupTag() : base("g")
        {
            inner = new List<Tag>();
        }

        public GroupTag Add(Tag child)
        {
            inner.Add(child);
            return this;
        }

        public override string ToString()
        {
            Inner = string.Join(Environment.NewLine, inner);
            return base.ToString();
        }
    }
}
