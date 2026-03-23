using SokoSolve.LargeSearchSolver.Lookup;
using System.Collections.Generic;

namespace SokoSolve.LargeSearchSolver;

public class SolverCoordinatorFactory : ISolverCoordinatorFactory, ISolverComponent
{
    INodeHeap? heap = null;

    public SolverCoordinatorFactory()
    {
        Tags = new HashSet<string>() { "DefaultTags" };
        TagsEffective = new HashSet<string>();
    }

    public SolverCoordinatorFactory(string[] tags)
    {
        Tags = new HashSet<string>(tags);
        TagsEffective = new HashSet<string>();
    }

    public bool AltOrExperimental { get; set; }
    public bool MemorySaving { get; set; }
    public bool BaseLine { get; set; }
    public bool VeryLarge { get; set; }
    public bool UnitTest { get; set; }
    public IReadOnlySet<string> Tags { get; set; }

    public IReadOnlySet<string> TagsEffective { get; private set; }

    public bool HasTag(string tag) => TagsEffective.Contains(tag);

    public string GetComponentName() => GetType().Name;
    public string Describe()
    {
        return string.Join(',', TagsEffective);
    }

    public void InitComplete() => GenerateEffectiveTags();

    public T? GetInstance<T>(LSolverRequest req, string? name = null)
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
            if (name == null || name == "Forward")
            {
                if (HasTag("RevOnly")) return default(T);
                if (HasTag("FwdAlt"))
                {
                    ILNodeStructEvaluator dead = new LNodeStructEvaluatorForwardAlt();
                    return (T)dead;
                }
                if (HasTag("FwdStable"))
                {
                    ILNodeStructEvaluator dead = new LNodeStructEvaluatorForwardStable();
                    return (T)dead;
                }
                if (HasTag("Dead"))
                {
                    ILNodeStructEvaluator dead = new LNodeStructEvaluatorForwardDeadChecks();
                    return (T)dead;
                }
                ILNodeStructEvaluator l = AltOrExperimental
                    ? new LNodeStructEvaluatorForwardAlt()
                    : new LNodeStructEvaluatorForwardDeadChecks();
                return (T)l;
            }
            else if (name == "Reverse")
            {
                if (HasTag("FwdOnly")) return default(T);
                ILNodeStructEvaluator l = new LNodeStructEvaluatorReverse();
                return (T)l;
            }
        }
        if (typeof(T) == typeof(INodeHashCalculator))
        {
            if (HasTag("TEST"))
            {
                INodeHashCalculator lt = new NodeHashCalculator();
                return (T)lt;
            }
            INodeHashCalculator l = BaseLine
                ? new NodeHashCalculator()
                : new NodeHashSytemHashCode();
            return (T)l;
        }
        if (typeof(T) == typeof(INodeWatcher))
        {
            INodeWatcher st = new SolutionTracker();
            return (T)st;
        }
        throw new NotImplementedException(typeof(T).Name);
    }

    private void GenerateEffectiveTags()
    {
        var eff = new HashSet<string>( Tags );
        if (AltOrExperimental) eff.Add("EXPERIMENTAL");
        if (MemorySaving) eff.Add("MEMORY");
        if (VeryLarge) eff.Add("VERYLARGE");
        if (BaseLine) eff.Add("BASELINE");
        if (UnitTest) eff.Add("TEST");

        TagsEffective = eff;
    }
}

