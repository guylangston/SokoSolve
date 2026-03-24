namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupCompound : ILNodeLookup, ILNodeLookupNested, ISolverComponent
{
    readonly LNodeLookupBlackRedTree dynamicInitial;
    readonly List<ILNodeLookup> immutable = new();

    public LNodeLookupCompound(INodeHeap heap, NSContext context)
    {
        Heap = heap;
        Context = context;
        dynamicInitial = new LNodeLookupBlackRedTree(heap, context);
    }

    public string GetComponentName() => nameof(LNodeLookupCompound);
    public string Describe() => $"Dynamic={dynamicInitial.GetType().Name}({ThresholdDynamic}), Immutable[{typeof(LNodeLookupImmutable).Name}]";
    public bool IsThreadSafe => false;
    public INodeHeap Heap { get;  }
    public NSContext Context { get; }
    public int ThresholdDynamic { get; set; } = 1_000_000;

    public void Add(ref NodeStruct node)
    {
        dynamicInitial.Add(ref node);
        if (dynamicInitial.Count >= ThresholdDynamic)
        {
            // copy to immutable
            var data = new NodeIndex[dynamicInitial.Count];
            dynamicInitial.CopyTo(data);
            var dataLookup = new LNodeLookupImmutable(Heap, Context, data);
            immutable.Add(dataLookup);
            dynamicInitial.Clear();
        }
    }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        if (dynamicInitial.TryFind(ref find, out var m1))
        {
            matchNodeId = m1;
            return true;
        }

        foreach(var item in immutable)
        {
            if (item.TryFind(ref find, out var m2))
            {
                matchNodeId = m2;
                return true;
            }
        }

        matchNodeId = NodeStruct.NodeId_NULL;
        return false;
    }

    public IEnumerable<(string Desc, ILNodeLookup Inner)> GetNested()
    {
        yield return ("Dynamic", dynamicInitial);
        for(var cc=0; cc<immutable.Count; cc++)
        {
            yield return ($"Immutable[{cc,2}]", immutable[cc] );
        }
    }
}

