using System.Runtime.InteropServices;

namespace SokoSolve.LargeSearchSolver;

public class NodeHeap : INodeHeap
{
    const int blockSize = 1_000_000;
    readonly List<NodeStruct[]> heapBlocks = new();
    readonly GCHandle handle;
    NodeStruct[] current;

    public NodeHeap()
    {
        current = new NodeStruct[blockSize];
        heapBlocks.Add(current);
        this.handle = GCHandle.Alloc(current, GCHandleType.Pinned);
    }
 
    volatile uint next = 0;

    public ref NodeStruct LeaseNode()
    {
        var curr = next;
        Interlocked.Increment(ref next);
        current[curr].SetNodeId(curr);
        return ref current[curr];
    }

    public void ReturnLease(uint nodeId)
    {
        throw new NotImplementedException();
    }

    public ref NodeStruct GetById(uint id)
    {
        return ref current[id];
    }

    public bool TryGetByHashCode(out uint matchNodeId)
    {
        throw new NotImplementedException();
    }

    ~NodeHeap()
    {
        this.handle.Free();
    }
}


