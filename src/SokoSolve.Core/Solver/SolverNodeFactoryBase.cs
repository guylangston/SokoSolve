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
        public abstract SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap);
        public abstract void       ReturnInstance(SolverNode canBeReused);

        public IBitmap CreateBitmap(VectorInt2 size) => new Bitmap(size);
        public IBitmap CreateBitmap(IBitmap clone) => new Bitmap(clone);
        

        public  SolverNode CreateFromPush(IBitmap  nodeCrateMap, IBitmap walls, 
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
                fromPool.InitialiseInstance(p, push, eCrate, eMove);
                return fromPool;
            }
            
            var newCrate = CreateBitmap(nodeCrateMap);
            newCrate[pp]  = false;
            newCrate[ppp] = true;
            var newMove = CreateBitmap(nodeCrateMap.Size); 
            SolverHelper.FloodFillUsingWallAndCratesInline(walls, newCrate, pp, newMove);
            return  CreateInstance(p, push, newCrate, newMove);
        }
        
        public SolverNode CreateFromPull(IBitmap nodeCrateMap, IBitmap walls, 
            VectorInt2 pc, VectorInt2 p, VectorInt2 pp)
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
                fromPool.InitialiseInstance(p, pp-p, eCrate, eMove);
                return fromPool;
            }
            
            var newCrate = CreateBitmap(nodeCrateMap);
            newCrate[pc] = false;
            newCrate[p]  = true;
            var newMove = CreateBitmap(nodeCrateMap.Size);
            SolverHelper.FloodFillUsingWallAndCratesInline(walls, newCrate, pp, newMove);
            return CreateInstance(p, pp - p, newCrate, newMove);
        }
        

        public virtual string  TypeDescriptor => $"{GetType().Name}";
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => ImmutableArray<(string name, string text)>.Empty;
    }
}