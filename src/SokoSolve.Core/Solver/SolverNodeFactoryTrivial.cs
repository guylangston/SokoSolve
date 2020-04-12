using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeFactoryTrivial : ISolverNodeFactory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, Bitmap crateMap, Bitmap moveMap) 
            => new SolverNode(player, push, crateMap, moveMap);

        public void ReturnInstance(SolverNode canBeReused)
        {
            // Do Nothing
        }
        
        public string                                  TypeDescriptor                             => $"{GetType().Name}";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => ImmutableArray<(string name, string text)>.Empty;
    }
}