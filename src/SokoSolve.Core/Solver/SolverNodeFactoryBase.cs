using System.Collections.Generic;
using System.Collections.Immutable;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public enum ForwardReverse
    {
        Forward,
        Reverse
    }
    
    public abstract class SolverNodeFactoryBase : ISolverNodeFactory
    {
        public virtual bool TryGetPooledInstance(ForwardReverse type, out SolverNode node)
        {
            node = null;
            return false;
        }
        public abstract SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, Bitmap crateMap, Bitmap moveMap);
        public abstract void       ReturnInstance(SolverNode canBeReused);

        public  SolverNode CreateFromPush(
            IBitmap  nodeCrateMap, IBitmap walls, 
            VectorInt2 p, VectorInt2 pp, VectorInt2 ppp, VectorInt2 push)
        {
            
            if (TryGetPooledInstance(ForwardReverse.Forward, out var fromPool))
            {
                var eCrate = fromPool.CrateMap;
                var eMove = fromPool.MoveMap;
                
                // Reuse the nodes, resetting the values
                eCrate.Set(nodeCrateMap);
                eCrate[pp]  = false;
                eCrate[ppp] = true;
                    
                SolverHelper.FloodFillUsingWallAndCratesInline(walls, eCrate, pp, eMove);
                fromPool.InitialiseInstance(p, push, eCrate, eMove);
                return fromPool;
            }
            
            var newCrate = new Bitmap(nodeCrateMap);
            newCrate[pp]  = false;
            newCrate[ppp] = true;
            var newMove = SolverHelper.FloodFillUsingWallAndCrates(walls, newCrate, pp);
            return  CreateInstance(p, push, newCrate, newMove);
        }
        
        public SolverNode CreateFromPull(
            IBitmap nodeCrateMap, IBitmap walls, 
            VectorInt2         pc, VectorInt2 p, VectorInt2 pp)
        {
            if (TryGetPooledInstance(ForwardReverse.Forward, out var fromPool))
            {
                var eCrate = fromPool.CrateMap;
                var eMove  = fromPool.MoveMap;
                
                // Reuse the nodes, resetting the values
                eCrate.Set(nodeCrateMap);
                eCrate[pc]  = false;
                eCrate[p] = true;
                    
                SolverHelper.FloodFillUsingWallAndCratesInline(walls, eCrate, pp, eMove);
                fromPool.InitialiseInstance(p, pp-p, eCrate, eMove);
                return fromPool;
            }
            
            var newCrate = new Bitmap(nodeCrateMap);
            newCrate[pc] = false;
            newCrate[p]  = true;
            var newMove = SolverHelper.FloodFillUsingWallAndCrates(walls, newCrate, pp);
            return CreateInstance(p, pp - p, newCrate, newMove);
        }
        

        public virtual string  TypeDescriptor => $"{GetType().Name}";
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => ImmutableArray<(string name, string text)>.Empty;
    }
}