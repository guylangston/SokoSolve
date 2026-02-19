using System.Runtime.InteropServices;

namespace SokoSolve.LargeSearchSolver;

public class NodeHeap : INodeHeap
{
    readonly Lock locker = new();
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

    public uint PeekNext() => next;

    public ref NodeStruct Lease()
    {
        lock(locker)
        {
            var curr = next;
            Interlocked.Increment(ref next);

            ref var node = ref current[curr];
            node.Reset();
            node.SetNodeId(curr);
            node.SetStatus(NodeStatus.LEASED);
            return ref node;
        }
    }

    public Span<NodeStruct> Lease(uint count)
    {
        lock(locker)
        {
            var start = next;
            var curr = next;
            var cc = 0;
            while(cc < count)
            {
                curr = next;
                Interlocked.Increment(ref next);

                ref var node = ref current[curr];
                node.Reset();
                node.SetNodeId(curr);
                node.SetStatus(NodeStatus.LEASED);

                cc++;
            }

            return current.AsSpan()[(int)start..((int)curr+1)];
        }
    }

    public void Return(uint nodeId)
    {
        throw new NotImplementedException();
    }

    public ref NodeStruct GetById(uint id)
    {
        if (id == uint.MaxValue) throw new ArgumentException("MaxValue is never a valid id. Is this Root.Parent, or similar");
        return ref current[id];
    }

    public bool TryGetByHashCode(ref NodeStruct find, out uint matchNodeId)
    {
        for(int cc=0; cc<next; cc++)    // lock?
        {
            if (current[cc].NodeId == find.NodeId) continue;

            if (current[cc].HashCode == find.HashCode)
            {

                // possible match
                if(current[cc].Equals(find))
                {
                    matchNodeId = current[cc].NodeId;
                    return true;
                }
            }
        }
        matchNodeId = uint.MaxValue;
        return false;

    }

    ~NodeHeap()
    {
        this.handle.Free();
    }
}


