namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupBlackRedTree : ILNodeLookup
{
    readonly SortedDictionary<int, Bucket> inner = new();

    class Bucket
    {
        public required uint FirstMatch { get; init; }
        public List<uint>? More { get; set; }
    }

    public LNodeLookupBlackRedTree(INodeHeap heap)
    {
        Heap = heap;
    }

    public bool IsThreadSafe => false;

    public INodeHeap Heap { get; }

    public void Add(ref NodeStruct node)
    {
        if (inner.TryGetValue(node.HashCode, out var bucket))
        {
            if (bucket.More == null)
            {
                bucket.More = [ node.NodeId ];
            }
            else
            {
                bucket.More.Add(node.NodeId);
            }
        }
        else
        {
            inner.Add(node.HashCode, new Bucket { FirstMatch = node.NodeId } );
        }

    }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        if (inner.TryGetValue(find.HashCode, out var bucket))
        {
            ref var first = ref Heap.GetById(bucket.FirstMatch);
            if (find.EqualsByRef(ref first))
            {
                matchNodeId = first.NodeId;
                return true;
            }
            if (bucket.More != null)
            {
                foreach(var mm in bucket.More)
                {
                    ref var mmStruct = ref Heap.GetById(mm);
                    if (find.EqualsByRef(ref mmStruct))
                    {
                        matchNodeId = mmStruct.NodeId;
                        return true;
                    }
                }
            }
        }

        matchNodeId = NodeStruct.NodeId_NULL;
        return false;
    }
}


