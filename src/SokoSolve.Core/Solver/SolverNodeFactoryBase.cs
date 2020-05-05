using System.Collections.Generic;
using System.Collections.Immutable;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverNodeFactoryBase : ISolverNodeFactory
    {
        
        public virtual bool TryGetPooledInstance( out SolverNode node)
        {
            node = null;
            return false;
        }

        public abstract SolverNode CreateInstance(SolverNode parent, VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap);
        public abstract void       ReturnInstance(SolverNode canBeReused);

        public abstract IBitmap CreateBitmap(VectorInt2 size);
        public abstract IBitmap CreateBitmap(IBitmap clone);

        public  SolverNode CreateFromPush(
            SolverNode parent,
            IBitmap  nodeCrateMap, IBitmap walls, 
            VectorInt2 p, VectorInt2 pp, VectorInt2 ppp, VectorInt2 push)
        {
            
            if (TryGetPooledInstance( out var fromPool))
            {
                var eCrate = fromPool.CrateMap;
                var eMove = fromPool.MoveMap;
                eMove.Fill(false);
                
                // Reuse the nodes, resetting the values
                eCrate.Set(nodeCrateMap);
                eCrate[pp]  = false;
                eCrate[ppp] = true;
                    
                
                SolverHelper.FloodFillUsingWallAndCratesInline(walls, eCrate, pp, eMove);
                fromPool.InitialiseInstance(parent, p, push, eCrate, eMove, true);
                return fromPool;
            }
            
            var newCrate = CreateBitmap(nodeCrateMap);
            newCrate[pp]  = false;
            newCrate[ppp] = true;

            var newMove = CreateBitmap(nodeCrateMap.Size);
            SolverHelper.FloodFillUsingWallAndCratesInline(walls, newCrate, pp, newMove);
            
            return  CreateInstance(parent, p, push, newCrate, newMove);
        }
        
        public SolverNode CreateFromPull(
            SolverNode parent,
            IBitmap nodeCrateMap, IBitmap walls, 
            VectorInt2         pc, VectorInt2 p, VectorInt2 pp)
        {
            if (TryGetPooledInstance( out var fromPool))
            {
                var eCrate = fromPool.CrateMap;
                var eMove  = fromPool.MoveMap;
                eMove.Fill(false);
                
                // Reuse the nodes, resetting the values
                eCrate.Set(nodeCrateMap);
                eCrate[pc]  = false;
                eCrate[p] = true;
                
                SolverHelper.FloodFillUsingWallAndCratesInline(walls, eCrate, pp, eMove);
                fromPool.InitialiseInstance(parent, p, pp-p, eCrate, eMove, true);
                return fromPool;
            }
            
            var newCrate = CreateBitmap(nodeCrateMap);
            newCrate[pc] = false;
            newCrate[p]  = true;

            var newMove = CreateBitmap(nodeCrateMap.Size);
            SolverHelper.FloodFillUsingWallAndCratesInline(walls, newCrate, pp, newMove);
            
            return CreateInstance(parent, p, pp - p, newCrate, newMove);
        }
        

        public virtual string  TypeDescriptor => $"{GetType().Name}";
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => ImmutableArray<(string name, string text)>.Empty;
    }
}