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
            heap = new NodeHeap(VeryLarge ? 10_000_000 : NodeHeap.DefaultSize );
            return (T)heap;
        }

        if (typeof(T) == typeof(INodeBacklog))
        {
            INodeBacklog bl = new NodeBacklog(VeryLarge ? 1_000_000 : NodeBacklog.DefaultSeedSize);
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
            if (VeryLarge)
            {
                // const int totalFastNodes = 16_000_000;
                // const int shards = 64;
                const int totalFastNodes = 5_000_000;
                const int shards = 16;
                ILNodeLookup l = new LNodeLookupSharding(heap, shards,
                        heap=>new LNodeLookupCompoundResize(heap)
                        {
                            ThresholdDynamic = totalFastNodes / shards
                        });
                return (T)l;
            }
            if (MemorySaving)
            {
                ILNodeLookup l = new LNodeLookupCompound(heap);
                return (T)l;
            }
            else
            {
                ILNodeLookup l = new LNodeLookupBlackRedTree(heap);
                return (T)l;
            }
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

