
namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupCompound : ILNodeLookup
{
    readonly LNodeLookupBlackRedTree dynamicInitial;
    readonly List<ILNodeLookup> immutable = new();

    public LNodeLookupCompound(INodeHeap heap)
    {
        Heap = heap;
        dynamicInitial = new LNodeLookupBlackRedTree(heap);
    }

    public bool IsThreadSafe => false;
    public INodeHeap Heap { get;  }
    public int ThresholdDynamic { get; set; } = 500_000;

    public void Add(ref NodeStruct node)
    {
        dynamicInitial.Add(ref node);
        if (dynamicInitial.Count >= ThresholdDynamic)
        {
            // copy to immutable
            var data = new NodeIndex[dynamicInitial.Count];
            dynamicInitial.CopyTo(data);
            var dataLookup = new LNodeLookupImmutable(Heap, data);
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
}



