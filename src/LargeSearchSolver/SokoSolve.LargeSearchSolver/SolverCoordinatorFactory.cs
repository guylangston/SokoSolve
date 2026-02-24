using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver;

public class SolverCoordinatorFactory : ISolverCoordinatorFactory
{
    INodeHeap? heap = null;
    public bool Experimental { get; set; }

    public T GetInstance<T>(LSolverRequest req)
    {
        if (typeof(T) == typeof(INodeHeap))
        {
            heap = new NodeHeap();
            return (T)heap;
        }

        if (typeof(T) == typeof(INodeBacklog))
        {
            INodeBacklog bl = new NodeBacklog();
            return (T)bl;
        }

        if (typeof(T) == typeof(ILNodeLookup))
        {
            if (heap == null) throw new InvalidDataException("heap should be assigned by now");
            ILNodeLookup l = new LNodeLookupCompound(heap);
            return (T)l;
        }

        if (typeof(T) == typeof(ILNodeStructEvaluator))
        {
            ILNodeStructEvaluator l = Experimental 
                ? new LNodeStructEvaluatorForwardExperimental()
                : new LNodeStructEvaluatorForwardStable();
            return (T)l;
        }
        if (typeof(T) == typeof(INodeHashCalculator))
        {
            INodeHashCalculator l = new NodeHashSytemHashCode();
            return (T)l;
        }
        throw new NotImplementedException(typeof(T).Name);
    }
}

