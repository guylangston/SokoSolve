using System.Collections.Generic;
using System.Linq;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public class SimpleArgMeta
    {
        public SimpleArgMeta(string name, string? s, string description, object? def = null, bool required = false, object tag = null)
        {
            Name        = name;
            Short       = s;
            Description = description;
            Default     = def;
            Required    = required;
            Tag         = tag;
        }

        public string  Name        { get; }    // --Name
        public string? Short       { get; }   // -s
        public string  Description { get; }
        public object? Default     { get; }
        public bool    Required    { get; }
        
        public object? Tag { get; set; }
    }

    public class SimpleArgs  : Dictionary<string, string>
    {
        public SimpleArgs()
        {
        }
        
        public SimpleArgs(IDictionary<string, string> dictionary) : base(dictionary)
        {
        }

        public static SimpleArgs FromMeta(IEnumerable<SimpleArgMeta> items)
        {
            var res = new SimpleArgs();
            foreach (var item in items)
            {
                if (item.Default != null)
                {
                    res[item.Name] = item.Default.ToString();    
                }
            }
            return res;
        }
        
        public static SimpleArgs Create(IEnumerable<SimpleArgMeta> defaults, IReadOnlyDictionary<string, string> args)
        {
            var res = FromMeta(defaults);
            foreach (var pair in args)
            {
                res[pair.Key] = pair.Value;
            }
            return res;
        }
        
        
        public new void Add(string key, string value)  // Allow Constructors to replace values
        {
            base[key] = value;
        }
        
        
        public string GenerateCommandLine(IReadOnlyDictionary<string, string>? defaults = null) => 
            FluentString.Join(this.Where(x => {
                    if (defaults != null && defaults.TryGetValue(x.Key, out var y))
                    {
                        return y != x.Value;
                    }
                    return true;
                }),
                new JoinOptions()
                {
                    Sep       = " ",
                    WrapAfter = 100
                }, (s, pair) => s.Append($"--{pair.Key} {pair.Value}"));

        
        
        public static string GenerateCommandLine(IReadOnlyDictionary<string, string> defaults,  IReadOnlyDictionary<string, string> args) => 
            FluentString.Join(args.Where(x => {
                    if (defaults.TryGetValue(x.Key, out var y))
                    {
                        return y != x.Value;
                    }
                    return true;
                }),
                new JoinOptions()
                {
                    Sep       = " ",
                    WrapAfter = 100
                }, (s, pair) => s.Append($"--{pair.Key} {pair.Value}"));
        
        
        public static void SetFromCommandLine(Dictionary<string, string> aa, string[] args, string verb)
        {
            var cc = 0;
            while (cc < args.Length)
            {
                if (args[cc].StartsWith("--"))
                {
                    var name = args[cc].Remove(0, 2);
                    if (cc + 1 < args.Length && !args[cc+1].StartsWith("--"))
                    {
                        aa[name] = args[cc + 1];
                        cc++;
                    }
                    else
                    {
                        aa[name] = true.ToString();  // flag
                    }
                }
                else if (cc == 0)
                {
                    aa[verb] = args[0]; // default 1st puzzle
                }
                cc++;
            }
        }

        public static SimpleArgs FromMetaAndCommandLine(IReadOnlyList<SimpleArgMeta> arguments, string[] args, string verb, out SimpleArgs defaults)
        {
            defaults = FromMeta(arguments);
            var res = new SimpleArgs(defaults);
            if (args is {Length: >0})
            {
                SetFromCommandLine(res, args, verb);    
            }
            
            return res;
        }
        
        public double? GetDouble(string name)
        {
            if (TryGetValue(name, out var vv) && double.TryParse(vv, out var vvv))
            {
                return vvv;
            }
            return null;
        }
        
        
        public int? GetInteger(string name)
        {
            if (TryGetValue(name, out var vv) && int.TryParse(vv, out var vvv))
            {
                return vvv;
            }
            return null;
        }
        
        public bool? GetBool(string name)
        {
            if (TryGetValue(name, out var vv) && bool.TryParse(vv, out var vvv))
            {
                return vvv;
            }
            return null;
        }
    }
}