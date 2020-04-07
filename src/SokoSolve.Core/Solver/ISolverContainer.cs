using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public interface ISolverContainer
    {
        bool TryGetInstance(Type type, out object instance);
    }

  
    
    public static class SolverContainer
    {
        public static bool TryGetInstance<T>(this ISolverContainer c, out T instance)
        {
            if (c.TryGetInstance(typeof(T), out var ii))
            {
                instance = (T) ii;
                return true;
            }

            instance = default(T);
            return false;
        }

        public static T GetInstance<T>(this ISolverContainer c)
        {
            if (c.TryGetInstance(typeof(T), out var instance))
            {
                return (T)instance;
            }

            return default;
        }
        
        public static T GetInstanceElseDefault<T>(this ISolverContainer c, Func<T> getDefault)
        {
            if (c != null && c.TryGetInstance(typeof(T), out var instance))
            {
                return (T)instance;
            }

            return getDefault();
        }
    }
    
    public class SolverContainerByType : ISolverContainer
    {
        private readonly IReadOnlyDictionary<Type, Func<Type, object>> factory;

        public SolverContainerByType(IReadOnlyDictionary<Type, Func<Type, object>> factory)
        {
            this.factory = factory;
        }

        public bool TryGetInstance(Type type, out object instance)
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