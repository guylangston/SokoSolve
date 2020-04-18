using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace SokoSolve.Core.Common
{
    public class FluentStringBuilder
    {
        private readonly StringBuilder sb = new StringBuilder();
        private string sep = ", ";

        public FluentStringBuilder()
        {
        }

        public FluentStringBuilder(string sep)
        {
            this.sep = sep;
        }

        public override string ToString() => sb.ToString();

        public FluentStringBuilder Append(string s)
        {
            sb.Append(s);
            return this;
        }
        
        public FluentStringBuilder AppendLine(string s)
        {
            sb.AppendLine(s);
            return this;
        }
        
        public FluentStringBuilder Sep(string? s = null)
        {
            if (sb.Length > 0 && !sb.ToString().EndsWith(s ?? sep)) sb.Append(s ?? sep);    // TODO: Refactor out ToString
            return this;
        }
        
        public FluentStringBuilder LF()
        {
            sb.AppendLine();
            return this;
        }
        
        public FluentStringBuilder IfNotNull<T>(T obj, Func<T, object> whenNotNull)
        {
            if (obj is null) return this;
            
            sb.Append(whenNotNull(obj)?.ToString());
            return this;
        }
        
        public FluentStringBuilder If(bool test, Func<object> then)
        {
            if (!test) return this;
            
            sb.Append(then()?.ToString());
            return this;
        }
        
        public FluentStringBuilder If(bool test, string then)
        {
            if (!test) return this;
            
            sb.Append(then);
            return this;
        }
        
        
        public FluentStringBuilder ForEach<T>(IEnumerable<T> items, Action<FluentStringBuilder, T> each)
        {
            foreach (var item in items)
            {
                each(this, item);
            }
            return this;
        }


        public FluentStringBuilder ForEach<T>(IEnumerable<T> items) =>
            ForEach(items, (fb, x) => fb.Sep().Append(x?.ToString()));
        

        public FluentStringBuilder Block(Action<FluentStringBuilder> block)
        {
            block(this);
            return this;
        }
        
        public FluentStringBuilder When(bool when, Action<FluentStringBuilder> then)
        {
            if (when) then(this);
            return this;
        }
        
        public static  implicit operator string(FluentStringBuilder ff) => ff.ToString();
        
       
    }
}