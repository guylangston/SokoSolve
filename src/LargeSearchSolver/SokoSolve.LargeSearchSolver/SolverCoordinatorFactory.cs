using SokoSolve.LargeSearchSolver.Lookup;

namespace SokoSolve.LargeSearchSolver;

public class SolverCoordinatorFactory : ISolverCoordinatorFactory
{
    INodeHeap? heap = null;
    public bool AltOrExperimental { get; set; }
    public bool MemorySaving { get; set; }
    public bool BaseLine { get; set; }
    public bool VeryLarge { get; set; }

    public T GetInstance<T>(LSolverRequest req)
    {
        if (typeof(T) == typeof(INodeHeap))
        {
            heap = new NodeHeap(VeryLarge ? 1_000_000 : NodeHeap.DefaultSize );
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
            if (BaseLine)
            {
                ILNodeLookup ll = new LNodeLookupLinkedList(heap);
                return (T)ll;
            }
            ILNodeLookup l = MemorySaving || VeryLarge
                ? new LNodeLookupCompound(heap)
                : new LNodeLookupBlackRedTree(heap);
            return (T)l;
        }

        if (typeof(T) == typeof(ILNodeStructEvaluator))
        {
            ILNodeStructEvaluator l = AltOrExperimental
                ? new LNodeStructEvaluatorForwardAlt()
                : new LNodeStructEvaluatorForwardStable();  // Also Baseline
            return (T)l;
        }
        if (typeof(T) == typeof(INodeHashCalculator))
        {
            INodeHashCalculator l = BaseLine
                ? new NodeHashCalculator()
                : new NodeHashSytemHashCode();
            return (T)l;
        }
        throw new NotImplementedException(typeof(T).Name);
    }
}

