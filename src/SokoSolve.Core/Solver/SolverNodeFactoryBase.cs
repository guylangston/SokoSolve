using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverNodeFactoryBaseDefault : SolverNodeFactoryBase
    {
        private readonly Func<IBitmap, IBitmap> factoryClone;
        private readonly Func<VectorInt2, IBitmap> factoryBySize;

        protected SolverNodeFactoryBaseDefault(string factoryArg)
        {
            switch (factoryArg.ToLowerInvariant())
            {
                case "default" :
                case "bitmap" :
                    factoryClone  = x => new Bitmap(x);
                    factoryBySize = x => new Bitmap(x);
                    break;
                
                case "byteseq":
                    factoryClone  = x => new BitmapByteSeq(x);;
                    factoryBySize = x => new BitmapByteSeq(x);
                    break;
                
                default:
                    throw new ArgumentException(factoryArg);
            }
        }
        protected SolverNodeFactoryBaseDefault() : this("default") { }

        protected SolverNodeFactoryBaseDefault(Func<IBitmap, IBitmap> factoryClone, Func<VectorInt2, IBitmap> factoryBySize)
        {
            this.factoryClone = factoryClone;
            this.factoryBySize = factoryBySize;
        }

       

        public override IBitmap CreateBitmap(IBitmap clone) => this.factoryClone(clone);
        public override IBitmap CreateBitmap(VectorInt2 size) => this.factoryBySize(size);
    }
    
    public abstract class SolverNodeFactoryBase : ISolverNodeFactory
    {
        
        public virtual bool TryGetPooledInstance( out SolverNode node)
        {
            node = null;
            return false;
        }

        public abstract SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap);
        public abstract void       ReturnInstance(SolverNode canBeReused);

        public abstract IBitmap CreateBitmap(VectorInt2 size);
        public abstract IBitmap CreateBitmap(IBitmap clone);

        public  SolverNode CreateFromPush(
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
        
        public SolverNode CreateFromPull(
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