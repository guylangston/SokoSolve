using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;

namespace SokoSolve.Core.Lib
{
    [Serializable]
    public class TrivialNameValueFileFormat : Dictionary<string, string>
    {
        public const string Comment = "#";
        public const string Seperator = "=";

        public const string Header =
            "# TrivialNameValueFileFormat (.tnv); lines in the form 'Property.Property.Name=Value<CRLF>'";

        private TrivialNameValueFileFormat()
        {
        }

        public static Tuple<string, string> SimpleSplit(string source, int position)
        {
            if (position < 0) return new Tuple<string, string>(source, null); // no seperator: string is nameonlt
            return new Tuple<string, string>(source.Substring(0, position),
                source.Substring(position + 1, source.Length - position - 1));
        }

        public static TrivialNameValueFileFormat Load(string fileName)
        {
            var dict = new TrivialNameValueFileFormat();
            foreach (var line in File.ReadAllLines(fileName))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(Comment)) continue;

                var idx = SimpleSplit(line, line.IndexOf(Seperator));
                if (idx != null) dict.Add(idx.Item1, idx.Item2);
            }

            return dict;
        }

        public void Save(string fileName)
        {
            using (var tx = new StreamWriter(fileName))
            {
                tx.WriteLine(Header);
                foreach (var key in Keys)
                {
                    tx.Write(key);
                    tx.Write(Seperator);
                    tx.WriteLine(this[key]);
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Header);
            foreach (var key in Keys)
            {
                sb.Append(key);
                sb.Append(Seperator);
                sb.AppendLine(this[key]);
            }

            return sb.ToString();
        }

        public static TrivialNameValueFileFormat Serialise<T>(T lib)
        {
            var res = new TrivialNameValueFileFormat();

            SerialiseProps(res, "", lib);

            return res;
        }

        private static void SerialiseProps(TrivialNameValueFileFormat res, string prefix, object obj)
        {
            if (obj == null) return;
            var t = obj.GetType();
            var props = t.GetProperties();
            foreach (var prop in props)
            {
                var v = prop.GetValue(obj, null);
                if (v == null) continue;

                var vt = v.GetType();
                if (vt == typeof(string))
                    res.Add(prefix + prop.Name, (string) v);
                else if (vt == typeof(int))
                    res.Add(prefix + prop.Name, v.ToString());
                else if (vt == typeof(DateTime))
                    res.Add(prefix + prop.Name, v.ToString());
                else if (vt == typeof(TimeSpan))
                    res.Add(prefix + prop.Name, v.ToString());
                else if (vt == typeof(bool))
                    res.Add(prefix + prop.Name, v.ToString());
                else if (vt == typeof(int))
                    res.Add(prefix + prop.Name, v.ToString());
                else
                    SerialiseProps(res, string.IsNullOrWhiteSpace(prefix)
                        ? prop.Name + "."
                        : prefix + "." + prop.Name + ".", v);
            }
        }


        public class WithBinder<T>
        {
            private T container;
            private string prefix;

            public WithBinder<T> SetWhen<TP>(KeyValuePair<string, string> pair, Expression<Func<T, TP>> selector,
                Func<string, TP> convert = null)
            {
                return SetWhen(pair, default, selector, convert);
            }

            public WithBinder<T> SetWhen<TP>(KeyValuePair<string, string> pair, T target,
                Expression<Func<T, TP>> selector, Func<string, TP> convert = null)
            {
                if (target == null) target = container;
                var getProp = ExpressionHelper<T>.GetPropertyName(selector);

                if (prefix != null) getProp = prefix + "." + getProp;
                if (getProp == pair.Key)
                {
                    var setter = ExpressionHelper<T>.BuildLamdaObjectSetter(selector);
                    if (convert != null)
                    {
                        setter(target, convert(pair.Value));
                    }
                    else
                    {
                        if (pair.Value != null)
                        {
                            if (typeof(TP) == typeof(string))
                                setter(target, (TP) (object) pair.Value);
                            else if (typeof(TP) == typeof(DateTime))
                                setter(target, (TP) (object) DateTime.Parse(pair.Value));
                            else if (typeof(TP) == typeof(TimeSpan))
                                setter(target, (TP) (object) TimeSpan.Parse(pair.Value));
                            else if (typeof(TP) == typeof(bool))
                                setter(target, (TP) (object) bool.Parse(pair.Value));
                            else if (typeof(TP) == typeof(int))
                                setter(target, (TP) (object) int.Parse(pair.Value));
                            else
                                throw new NotImplementedException();
                        }
                    }
                }

                return this;
            }

            public WithBinder<TB> With<TB>(T data, Expression<Func<T, TB>> inner)
            {
                var c = inner.Compile()(data);
                var pre = ExpressionHelper<T>.GetPropertyName(inner);
                return new WithBinder<TB>
                {
                    container = c,
                    prefix = pre
                };
            }
        }
    }
}