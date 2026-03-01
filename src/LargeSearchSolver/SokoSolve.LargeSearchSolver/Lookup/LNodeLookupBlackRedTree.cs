using System.Diagnostics;

namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupBlackRedTree : ILNodeLookup, ILNodeLookupStats
{
    readonly SortedDictionary<int, Bucket> inner = new();

    class Bucket
    {
        public required uint FirstMatch { get; init; }
        public List<uint>? CollisionMatches { get; set; }
    }

    public LNodeLookupBlackRedTree(INodeHeap heap)
    {
        Heap = heap;
    }

    public bool IsThreadSafe => false;
    public string GetComponentName() => GetType().Name;
    public string Describe() => "";

    public INodeHeap Heap { get; }
    public int Count { get; private set; }
    public ulong LookupsTotal { get; private set; }
    public int Collisons { get; private set; }

    public void Add(ref NodeStruct node)
    {
        if (inner.TryGetValue(node.HashCode, out var bucket))
        {
            if (bucket.CollisionMatches == null)
            {
                bucket.CollisionMatches = [ node.NodeId ];
            }
            else
            {
                Collisons++;
                ListHelper.InsertSorted(bucket.CollisionMatches, node.NodeId, (a,b)=>a.CompareTo(b));
            }
        }
        else
        {
            inner.Add(node.HashCode, new Bucket { FirstMatch = node.NodeId } );
        }
        Count++;
    }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        LookupsTotal++;
        if (inner.TryGetValue(find.HashCode, out var bucket))
        {
            ref var first = ref Heap.GetById(bucket.FirstMatch);
            if (find.EqualsByRef(ref first))
            {
                matchNodeId = first.NodeId;
                return true;
            }
            if (bucket.CollisionMatches != null)
            {
                foreach(var mm in bucket.CollisionMatches)
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

    public void Clear()
    {
        inner.Clear();
        Count = 0;
    }

    public void CopyTo(NodeIndex[] target)
    {
        Debug.Assert(target.Length >= Count);
        var cc=0;
        foreach(var x in inner)
        {
            target[cc++] = new NodeIndex(x.Value.FirstMatch, x.Key);
            if (x.Value.CollisionMatches != null)
            {
                foreach(var secondary in x.Value.CollisionMatches)
                {
                    target[cc++] = new NodeIndex(x.Value.FirstMatch, x.Key);
                }
            }
        }

    }
}


