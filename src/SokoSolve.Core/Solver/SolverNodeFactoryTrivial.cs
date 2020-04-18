using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeFactoryTrivial : SolverNodeFactoryBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap) 
            => new SolverNode(player, push, crateMap, moveMap);

        public override void ReturnInstance(SolverNode canBeReused)
        {
            // Do Nothing
        }
        
        
    }
}