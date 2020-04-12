using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class SolverNodeFactoryPooling : ISolverNodeFactory
    {
        
        private const    int          MaxPool     = 2048;
        private readonly SolverNode[] buffer      = new SolverNode[MaxPool];
        private volatile int          bufferIndex = -1;
        private volatile int          refused     = 0;
        
        public string                                  TypeDescriptor                             => $"{GetType().Name}[{MaxPool}]";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state) => ImmutableArray<(string name, string text)>.Empty;

        private volatile bool readSpinLock;
        public SolverNode CreateInstance(VectorInt2 player, VectorInt2 push, Bitmap crateMap, Bitmap moveMap)
        {
            while(readSpinLock) { }
            readSpinLock = true;
            try
            {

                var h = Interlocked.Decrement(ref bufferIndex);
                if (h < 0)
                {
                    bufferIndex = -1; // reset to start state (empty buffer)
                    return new SolverNode(player, push, crateMap, moveMap);    
                }
                else
                {
                    if (h >= MaxPool - 1)
                    {
                        return new SolverNode(player, push, crateMap, moveMap);
                    }
                    if (buffer[h] == null)
                    {
                        // Thread contention?
                        // Could try again, but that may cause StackOverflow,
                        // So safer to just issue a new obj
                        return new SolverNode(player, push, crateMap, moveMap);   
                    }

                    var reuse = buffer[h];
                    reuse.InitialiseInstance(player, push, crateMap, moveMap);
                    buffer[h] = null;
                    return reuse;
                }
            }
            finally
            {
                readSpinLock = false;
            }
            
            
        }

      

        public void ReturnInstance(SolverNode canBeReused)
        {
            while(readSpinLock) { }
            var next = Interlocked.Increment(ref bufferIndex);
            if (next >= 0 && next < MaxPool)
            {
                buffer[next] = canBeReused;
            }
            else
            {
                Interlocked.Increment(ref refused);
            }
        }
    }
}