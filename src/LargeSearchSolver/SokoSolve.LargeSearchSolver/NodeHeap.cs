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
            return ref LeaseInner();
        }
    }

    int countLease = 0;
    int countReturn = 0;

    public int StatsCountLease => countLease;
    public int StatsCountReturn => countReturn;

    ref NodeStruct LeaseInner()
    {
        Interlocked.Increment(ref countLease);

        var curr = next;
        Interlocked.Increment(ref next);

        ref var node = ref current[curr];
        node.Reset();
        node.SetNodeId(curr);
        node.SetStatus(NodeStatus.LEASED);
        return ref node;
    }


    // NOTE: BROKEN! Span may include items for reuse
    // public Span<NodeStruct> Lease(uint count)
    // {
    //     if (count == 0) throw new ArgumentException(null, nameof(count));
    //
    //     lock(locker)
    //     {
    //         var start = next;
    //         var curr = next;
    //         var cc = 0;
    //         while(cc < count)
    //         {
    //             curr = next;
    //             Interlocked.Increment(ref next);
    //
    //             ref var node = ref current[curr];
    //             node.Reset();
    //             node.SetNodeId(curr);
    //             node.SetStatus(NodeStatus.LEASED);
    //
    //             cc++;
    //         }
    //
    //         return current.AsSpan()[(int)start..((int)curr+1)];
    //     }
    // }

    Queue<uint> poolFree = new();
    public void Return(uint nodeId)
    {
        Interlocked.Increment(ref countReturn);
        poolFree.Enqueue(nodeId);
    }

    public ref NodeStruct GetById(uint id)
    {
        if (id == NodeStruct.NodeId_NULL) throw new InvalidDataException(nameof(NodeStruct.NodeId_NULL));
        if (id == NodeStruct.NodeId_NonPooled) throw new InvalidDataException(nameof(NodeStruct.NodeId_NonPooled));
        return ref current[id];
    }

    public bool TryGetByHashCode(ref NodeStruct find, out uint matchNodeId)
    {
        for(int cc=0; cc<next; cc++)    // lock?
        {
            ref var curr = ref current[cc];
            if (curr.HashCode == find.HashCode)
            {
                if (curr.NodeId == find.NodeId) continue;

                // possible match
                if(curr.IsMatch(ref find))
                {
                    matchNodeId = curr.NodeId;
                    return true;
                }
            }
        }
        matchNodeId = NodeStruct.NodeId_NULL;
        return false;

    }

    ~NodeHeap()
    {
        this.handle.Free();
    }
}


