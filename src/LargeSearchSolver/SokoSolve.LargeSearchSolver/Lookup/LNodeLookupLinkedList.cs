namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupLinkedList : ILNodeLookup, ILNodeLookupSelfCheck
{
    readonly LinkedList<NodeIndex> sorted = new();

    public LNodeLookupLinkedList(INodeHeap heap)
    {
        Heap = heap;
    }

    public INodeHeap Heap { get; }
    public bool IsThreadSafe => false;

    public void Add(ref NodeStruct node)
    {
        var idx = new NodeIndex(node.NodeId, node.HashCode);
        if (sorted.Count == 0)
        {
            sorted.AddFirst(idx);
            return;
        }

        var curr = sorted.First;
        while(curr != null && curr.Value.HashCode < node.HashCode)
        {
            curr = curr.Next;
        }

        if (curr == null)
        {
            sorted.AddLast(idx);
        }
        else
        {
            sorted.AddBefore(curr, idx);
        }
    }

    public void Check()
    {
        var curr = sorted.First;
        var idx = 0;
        while(curr != null)
        {
            if (curr.Previous != null)
            {
                if (curr.Previous.Value.HashCode > curr.Value.HashCode)
                    throw new InvalidDataException($"Not sorted at index {idx}");
            }
            curr = curr.Next;
            idx++;
        }
    }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        var curr = sorted.First;
        while(curr != null && curr.Value.HashCode < find.HashCode)
        {
            curr = curr.Next;
        }

        while(curr != null && curr.Value.HashCode == find.HashCode)
        {
            ref var currNode = ref Heap.GetById(curr.Value.NodeId);
            if (find.EqualsByRef(ref currNode))
            {
                matchNodeId = currNode.NodeId;
                return true;
            }
            curr = curr.Next;
        }

        matchNodeId = NodeStruct.NodeId_NULL;
        return false;
    }
}

