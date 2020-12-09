using System.Runtime.CompilerServices;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SolverNodePoolingFactoryDefault : SolverNodePoolingFactoryBaseDefault
    {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SolverNode CreateInstance(SolverNode parent, VectorInt2 player, VectorInt2 push, IBitmap crateMap, IBitmap moveMap) 
            => new SolverNode(parent, player, push, crateMap, moveMap);

        public override void ReturnInstance(SolverNode canBeReused)
        {
            // Do Nothing
        }
        
        
    }
}