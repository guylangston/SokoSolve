using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace TextRenderZ
{
    
    public class JoinOptions
    {
        public static JoinOptions Default = new JoinOptions()
        {
            WrapAfter = 0,
            SkipNull = false,
            Sep = ", "
            
        };

        public int    WrapAfter { get; set; }
        public bool   SkipNull  { get; set; }
        public string Sep       { get; set; }
    }
    
    public class FluentString
    {
        private readonly StringBuilder sb = new StringBuilder();
        private string sep;

        public FluentString() : this(", ") { }

        public FluentString(string sep)
        {
            this.sep = sep;
        }


        public override string ToString() => sb.ToString();
        public static  implicit operator string(FluentString ff) => ff.ToString();

        public FluentString Append(string? s)
        {
            if (s != null) sb.Append(s);
            return this;
        }
        
        public FluentString AppendLine(string? s)
        {
            if (s != null) sb.AppendLine(s);
            return this;
        }
        
        public FluentString Sep(string? s = null)
        {
            if (sb.Length > 0 && !sb.ToString().EndsWith(s ?? sep)) sb.Append(s ?? sep);    // TODO: Refactor out ToString
            return this;
        }
        
        public FluentString LF()
        {
            sb.AppendLine();
            return this;
        }
        
        public FluentString IfNotNull<T>(T obj, Func<T, object> whenNotNull)
        {
            if (obj is null) return this;
            
            sb.Append(whenNotNull(obj)?.ToString());
            return this;
        }
        
        public FluentString If(bool test, Func<object> then)
        {
            if (!test) return this;
            
            sb.Append(then()?.ToString());
            return this;
        }
        
        public FluentString If(bool test, string then)
        {
            if (!test) return this;
            
            sb.Append(then);
            return this;
        }

        public FluentString ForEach<T>(IEnumerable<T> items, Action<FluentString, T> each)
        {
            foreach (var item in items)
            {
                each(this, item);
            }
            return this;
        }

        public FluentString ForEach<T>(IEnumerable<T> items) =>
            ForEach(items, (fb, x) => fb.Sep().Append(x?.ToString()));
        

        public FluentString Block(Action<FluentString> block)
        {
            block(this);
            return this;
        }
        
        public FluentString When(bool when, Action<FluentString> then)
        {
            if (when) then(this);
            return this;
        }
        
        public FluentString UsingTextWriter(Action<TextWriter> textWriter)
        {
            using (var tw = new StringWriter(sb))
            {
                textWriter(tw);
            }

            return this;
        }
        
        public FluentString Quote(string s, char quote = '"')
        {
            sb.Append(quote);
            sb.Append(s);
            sb.Append(quote);
            return this;
        }
        
        //========================================================================================================
        
        
        public static FluentString Create() => new FluentString();
        
        public static FluentString CreateHtmlUl<T>(IEnumerable<T> items, Action<FluentString, T> each, string style = "ul-compact") 
            => new FluentString()
               .Append($"<ul class='{style}'>")
               .ForEach(items, (f, d) =>
               {
                   f.Append("<li>");
                   each(f, d);
                   f.AppendLine("</li>");
               })
               .Append("</ul>");

        
        public static FluentString Join<T>(IEnumerable<T> items) =>
            Join(items, JoinOptions.Default, (fs, item) => fs.Append(item?.ToString()) );
        
        public static FluentString Join<T>(IEnumerable<T> items, JoinOptions options) =>
            Join(items, options, (fs, item) => fs.Append(item?.ToString()) );
        
        public static FluentString Join<T>(IEnumerable<T> items, Func<T, object> each) =>
            Join(items, JoinOptions.Default, (fs, item) => fs.Append(each(item).ToString()) );

        public static FluentString Join<T>(IEnumerable<T> items, Action<FluentString, T> each) =>
            Join(items, JoinOptions.Default, each);

        public static FluentString Join<T>(IEnumerable<T> items, JoinOptions options, Action<FluentString, T> each)
            => new FluentString(options.Sep)
                .ForEach(items, (f, d) =>
                {
                    if (options.SkipNull && d == null) return;
                    f.Sep();
                    each(f, d);
                    if (options.WrapAfter > 0)
                    {
                        // TODO: Implement WrapAfter X chars
                    }
                    
                });

        

    }
}
