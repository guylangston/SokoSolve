using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public abstract class SolverNodeFactoryBaseDefault : SolverNodeFactoryBase, ISolveNodeFactoryPuzzleDependant
    {
        private readonly string                    factoryArg;
        private          Func<IBitmap, IBitmap>    factoryClone;
        private          Func<VectorInt2, IBitmap> factoryBySize;

        protected SolverNodeFactoryBaseDefault(string factoryArg)
        {
            this.factoryArg = factoryArg;
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
                
                case "index":
                    // See SetupForPuzzle
                    break;
                
                default:
                    throw new ArgumentException(factoryArg);
            }
        }
        protected SolverNodeFactoryBaseDefault() : this("default") { }

        protected SolverNodeFactoryBaseDefault(Func<IBitmap, IBitmap> factoryClone, Func<VectorInt2, IBitmap> factoryBySize)
        {
            this.factoryClone  = factoryClone;
            this.factoryBySize = factoryBySize;
        }

        public override IBitmap CreateBitmap(IBitmap clone) => this.factoryClone(clone);
        
        
        public virtual void SetupForPuzzle(Puzzle puzzle)
        {
            switch (factoryArg.ToLowerInvariant())
            {
                case "index": 
                    var master = new BitmapMaskedIndex.Master(puzzle.ToMap(puzzle.Definition.AllFloors));
                    factoryClone  = x => new BitmapMaskedIndex(master, x);;
                    factoryBySize = x => new BitmapMaskedIndex(master);
                    break;
            }
        }

        public override IBitmap CreateBitmap(VectorInt2 size) => this.factoryBySize(size);
        
        public override string  TypeDescriptor => $"{GetType().Name}[{factoryArg}]";
        public virtual IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => ImmutableArray<(string name, string text)>.Empty;
    }
}