using System.Diagnostics;

namespace SokoSolve.LargeSearchSolver;

public class NodeHeap : INodeHeap
{
    readonly Lock locker = new();
    readonly int blockSize;
    readonly List<Block> heapBlocks = new();
    readonly Queue<uint> poolFree = new();
    Block current;
    int countLease = 0;
    int countReturn = 0;
    volatile uint next = 0;

    class Block
    {
        public NodeStruct[] ByNodeId;
    }

    public NodeHeap(int blockSize = 100_000)
    {
        this.blockSize = blockSize;
        current = new Block()
        {
            ByNodeId = new NodeStruct[blockSize],
        };
        heapBlocks.Add(current);
    }

    public uint PeekNext() => next;

    public ref NodeStruct Lease()
    {
        lock(locker)
        {
            return ref LeaseInner();
        }
    }

    public int StatsCountLease => countLease;
    public int StatsCountReturn => countReturn;

    ref NodeStruct LeaseInner()
    {
        Interlocked.Increment(ref countLease);

        var currId = next;
        Interlocked.Increment(ref next);

        int blockIdx = (int)currId / blockSize;
        int arrayIdx = (int)currId % blockSize;
        if (blockIdx >= heapBlocks.Count)
        {
            current = new Block()
            {
                ByNodeId = new NodeStruct[blockSize]
            };
            heapBlocks.Add(current);
        }

        ref var node = ref current.ByNodeId[arrayIdx];
        node.Reset();
        node.SetNodeId(currId);
        node.SetStatus(NodeStatus.LEASED);
        return ref node;
    }


    public void Return(uint nodeId)
    {
        Interlocked.Increment(ref countReturn);
        poolFree.Enqueue(nodeId);
    }

    public void Commit(ref NodeStruct node)
    {
        Debug.Assert(node.Status == NodeStatus.NEW_CHILD);
        Debug.Assert(node.NodeId < NodeStruct.NodeId_NonPooled);
        Debug.Assert(node.ParentId < NodeStruct.NodeId_NonPooled);
        Debug.Assert(node.HashCode != 0);
    }

    public ref NodeStruct GetById(uint id)
    {
        if (id == NodeStruct.NodeId_NULL) throw new InvalidDataException(nameof(NodeStruct.NodeId_NULL));
        if (id == NodeStruct.NodeId_NonPooled) throw new InvalidDataException(nameof(NodeStruct.NodeId_NonPooled));

        int blockIdx = (int)id / blockSize;
        int arrayIdx = (int)id % blockSize;
        return ref heapBlocks[blockIdx].ByNodeId[arrayIdx];
    }

}


