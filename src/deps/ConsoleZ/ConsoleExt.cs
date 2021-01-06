using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleZ
{
    public static class ConsoleExt
    {
        public static int WriteLine(this IConsole cons) => cons.WriteLine(null);

        public static int WriteObj(this IConsole cons, object obj) => cons.WriteFormatted($"{obj}");

        public static int WriteException(this IConsole cons, Exception ex) => cons.WriteFormatted($"{ex}");

        public static int WriteLabel(this IConsole cons, string name, object val) => cons.WriteFormatted($"^yellow;{name,30}^; ^orange;|^; {val}");

        public static int WriteWarning(this IConsole cons, string lbl, string txt) => cons.WriteFormatted($"^red;WARNING:^;^orange;{lbl}.^; {txt}");

        public static int WriteLabel<T, TP>(this IConsole cons, T item, Expression<Func<T, TP>> exp)
        {
            var comp = exp.Compile();
            return WriteLabel(cons, GetPropertyInfo(exp)?.Name ?? exp.ToString(), comp(item));
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a property that is not from type {type}.");

            return propInfo;
        }
    }

    public static class DefaultConsole
    {
        public static IConsole Console { get; set; } = StdConsole.Singleton;
    }
}
