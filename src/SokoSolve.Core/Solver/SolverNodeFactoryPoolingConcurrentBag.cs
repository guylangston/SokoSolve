using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeFactoryPoolingConcurrentBag : ISolverNodeFactory
    {
        private          int                       hit     = 0;
        private          int                       miss    = 0;
        private          int                       refused = 0;
        private const    int                       MaxPool = 2048;
        private readonly ConcurrentBag<SolverNode> pool    = new ConcurrentBag<SolverNode>();
        
        
        public SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, Bitmap crateMap, Bitmap moveMap)
        {
            if (pool.Count > 0)
            {
                if (pool.TryTake(out var inst))
                {
                    hit++;
                    inst.InitialiseInstance(player, push, crateMap, moveMap);
                    return inst;
                }
            }

            miss++;
            return new SolverNode(player, push, crateMap, moveMap);
            
        }
        
        public void ReturnInstance(SolverNode canBeReused)
        {
            if (pool.Count < MaxPool)
            {
                pool.Add(canBeReused);
            }
            else
            {
                refused++;
            }
        }

        public string TypeDescriptor => $"{GetType().Name}[{MaxPool}]";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => ImmutableArray<(string name, string text)>.Empty;
    }
}