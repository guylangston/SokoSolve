using System;
using System.Collections.Concurrent;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeFactoryPoolingConcurrentBag : SolverNodeFactoryBaseDefault
    {
        private          int                       hit     = 0;
        private          int                       miss    = 0;
        private          int                       refused = 0;
        private const    int                       MaxPool = 2048;
        private ConcurrentBag<SolverNode> pool    = new ConcurrentBag<SolverNode>();

        public SolverNodeFactoryPoolingConcurrentBag(string factoryArg) : base(factoryArg)
        {
        }

        public SolverNodeFactoryPoolingConcurrentBag()
        {
        }

        public override void SetupForPuzzle(Puzzle puzzle)
        {
            pool = new ConcurrentBag<SolverNode>();
            base.SetupForPuzzle(puzzle);
        }

        public override  SolverNode CreateInstance(SolverNode parent, VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap)
        {
            if (pool.Count > 0)
            {
                if (pool.TryTake(out var inst))
                {
                    hit++;
                    inst.InitialiseInstance(parent, player, push, crateMap, moveMap, true);
                    return inst;
                }
            }

            miss++;
            return new SolverNode(parent, player, push, crateMap, moveMap);
            
        }
        
        public override void ReturnInstance(SolverNode canBeReused)
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

        public override bool TryGetPooledInstance( out SolverNode node)
        {
            if (pool.Count > 0)
            {
                if (pool.TryTake(out var inst))
                {
                    hit++;
                    node = inst;
                    return true;
                }
            }

            node = null;
            return false;
        }

        public override string TypeDescriptor => $"{GetType().Name}[{MaxPool}]";
        
    }
}