using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace SokoSolve.Core.Library
{
    public static class ExpressionHelper<TClass>
    {
        public static Action<TClass, object> BuildLamdaObjectSetter<TProp>(Expression<Func<TClass, TProp>> expression)
        {
            return ExpressionHelper.BuildLamdaObjectSetter(expression);
        }


        public static PropertyInfo GetProperty<TProp>(Expression<Func<TClass, TProp>> expression, string name)
        {
            var body = expression.Body as MemberExpression;
            if (body != null)
            {
                var prop = body.Member as PropertyInfo;
                if (prop != null)
                    if (prop.DeclaringType == typeof(TClass) && prop.Name == name)
                        return prop;
            }

            return null;
        }

        public static string GetPropertyName<TR>(Expression<Func<TClass, TR>> expression)
        {
            //See http://handcraftsman.wordpress.com/2008/11/11/how-to-get-c-property-names-without-magic-strings/
            var body = expression.Body as MemberExpression;
            if (body != null) return body.Member.Name;
            return null;
        }
    }

    public static class ExpressionHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            //See http://handcraftsman.wordpress.com/2008/11/11/how-to-get-c-property-names-without-magic-strings/
            var body = expression.Body as MemberExpression;
            if (body != null) return body.Member.Name;
            return null;
        }

        public static string GetPropertyName<T, TR>(Expression<Func<T, TR>> expression)
        {
            //See http://handcraftsman.wordpress.com/2008/11/11/how-to-get-c-property-names-without-magic-strings/
            var body = expression.Body as MemberExpression;
            if (body != null) return body.Member.Name;
            return null;
        }

        public static Type GetPropertyType(LambdaExpression expression)
        {
            return expression.Body.Type;
        }


        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetProperties().FirstOrDefault(x => x.Name == name);
        }

        public static Action<TClass, object> BuildLamdaObjectSetter<TClass, TProp>(
            Expression<Func<TClass, TProp>> selector)
        {
            var body = (MemberExpression) selector.Body;

            var parmClass = Expression.Parameter(typeof(TClass), "c");
            var parmAssign = Expression.Parameter(typeof(object), "o");

            var info = GetProperty(typeof(TClass), body.Member.Name);
            if (info != null)
            {
                // Simple case : c=>c.Prop becomes (c,o) => c.Prop = (T)o;
                var prop = Expression.Property(parmClass, info);
                var assign = Expression.Assign(prop, Expression.Convert(parmAssign, body.Type));

                var setProp = Expression.Lambda<Action<TClass, object>>(assign, parmClass, parmAssign);
                var setValue = setProp.Compile();

                return setValue;
            }

            // Nested case : c=>c.Nested.Prop becomes (c,o) => c.Nested.Prop = (T)o;
            var inner = body.Expression as MemberExpression;
            if (inner != null)
            {
                var firstProp = Expression.Property(parmClass, inner.Member.Name);
                var finalprop = Expression.Property(firstProp, body.Member.Name);

                var assign = Expression.Assign(finalprop, Expression.Convert(parmAssign, body.Type));

                var setProp = Expression.Lambda<Action<TClass, object>>(assign, parmClass, parmAssign);
                var setValue = setProp.Compile();

                return setValue;
            }

            return null; // Not Supported
        }
    }
}