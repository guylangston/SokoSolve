namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupImmutable : ILNodeLookup,ILNodeLookupStats, IComparer<NodeIndex>
{
    private readonly NodeIndex[] data;

    public LNodeLookupImmutable(INodeHeap heap, NodeIndex[] data)
    {
        Heap = heap;
        this.data = data;
    }

    public bool IsThreadSafe => true;
    public string GetComponentName() => GetType().Name;
    public string Describe() => "";

    public INodeHeap Heap { get; }

    public NodeIndex[] GetInnerArray() => data;

    public int Count => data.Length;

    ulong lookups;
    public ulong LookupsTotal => lookups;

    public int Collisons => -1;

    public void Add(ref NodeStruct node) => throw new NotImplementedException();

    public int Compare(NodeIndex x, NodeIndex y) => x.HashCode.CompareTo(y.HashCode);

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        lookups++;

        var idx = Array.BinarySearch(data, 0, data.Length, new NodeIndex(find.NodeId, find.HashCode), this);
        if (idx < 0)
        {
            matchNodeId = 0;
            return false;
        }

        // May not stop at first match
        while(idx > 0 &&  data[idx-1].HashCode == data[idx].HashCode)
        {
            idx--;
        }

        while(idx < data.Length && data[idx].HashCode == find.HashCode)
        {
            ref var node = ref Heap.GetById(data[idx].NodeId);
            if (find.EqualsByRef(ref node))
            {
                matchNodeId = node.NodeId;
                return true;
            }
            idx++;
        }

        matchNodeId = 0;
        return false;
    }
}




