using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SokoSolve.Core.Solver
{
    public interface ISolverContainer
    {
        bool TryGetInstance(Type type, out object instance);

        bool TryGetInstance<T>(out T instance)
        {
            if (TryGetInstance(typeof(T), out var obj))
            {
                instance = (T)obj;
                return true;
            }
            instance = default;
            return false;
        }
    }
    
    

  
    
    public static class SolverContainer
    {
        public static bool TryGetInstance<T>(this ISolverContainer c,out T instance) where T: class
        {
            if (c.TryGetInstance(typeof(T), out var ii))
            {
                instance = (T) ii;
                return true;
            }

            instance = default(T);
            return false;
        }

        public static T? GetInstance<T>(this ISolverContainer c)
        {
            if (c.TryGetInstance(typeof(T), out var instance))
            {
                return (T)instance;
            }

            return default;
        }
        
        public static T GetInstanceElseDefault<T>(this ISolverContainer c, Func<T> getDefault)
        {
            if (c is object && c.TryGetInstance(typeof(T), out var instance))
            {
                return (T)instance;
            }

            return getDefault();
        }
        
        
    }
    
    public class SolverContainerByType : ISolverContainer
    {
        private readonly ConcurrentDictionary<Type, Func<Type, object>> factory;

        public SolverContainerByType()
        {
            this.factory = new ConcurrentDictionary<Type, Func<Type, object>>();
        }

        public void Register(Type target, Func<Type, object> func) => factory[target] = func;
        
        public void Register<T>(Func<Type, object> func) => factory[typeof(T)] = func;

        public void Register(IDictionary<Type, Func<Type, object>> items)
        {
            foreach (var item in items)
            {
                Register(item.Key, item.Value);
            }
        }

        public static readonly ISolverContainer DefaultEmpty = new SolverContainerByType();  

        public bool TryGetInstance(Type type, [MaybeNullWhen(false)] out object instance)
        {
            if (factory.TryGetValue(type, out var ff))
            {
                instance = ff(type);
                return true;
            }

            instance = null;
            return false;
        }
    }
}