using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SokoSolve.Core.Primitives;

namespace SokoSolve.Tests
{
    public class TestReport 
    {
        readonly StringBuilder inner = new StringBuilder();

        public TestReport( )
        {
        }


        public TestReport(string expected)
        {
            inner.Append(expected);
        }

        public static string NormalizeLineFeed(string text)
        {
            return string.Concat(text.Split('\n')
                .Select(x => x.Trim('\r'))
                .Where(x=>!string.IsNullOrWhiteSpace(x))
                .Select(x=>x + Environment.NewLine));
        }

        public override bool Equals(object obj)
        {
            var x = (TestReport) obj;
            var a = NormalizeLineFeed(inner.ToString());
            var b = NormalizeLineFeed(x.ToString());
            return string.Equals(a, b, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return inner.ToString().GetHashCode();
        }


        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);

            inner.AppendFormat(format, args);
            inner.AppendLine();
        }


        public void WriteLine(object obj)
        {
            if (obj == null) return;

            Console.WriteLine(obj);

            inner.AppendFormat(obj.ToString());
            inner.AppendLine();
        }

        public override string ToString()
        {
            return inner.ToString();
        }

        public void WriteLineAll<T>(IEnumerable<T> res)
        {
            int cc = 0;
            foreach (var re in res)
            {
                WriteLine("{0,-3} {1}", cc++, re);
            }
        }

        public void WriteList<T>(IEnumerable<T> list)
        {
            var cc = 0;
            foreach (var item in list)
            {
                WriteLine("{0} Item {1}", typeof(T).Name, cc++);
                WriteLine(item);
            }
            
        }
    }
}