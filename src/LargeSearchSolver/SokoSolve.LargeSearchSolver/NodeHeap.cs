using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SokoSolve.LargeSearchSolver;

public class NodeHeap : INodeHeap
{
    readonly Lock locker = new();
    readonly int blockSize;
    readonly List<Block> heapBlocks = new();
    readonly Queue<uint> poolFree = new();
    Block current;

    class Block
    {
        public NodeStruct[] ByNodeId;
        public List<ByHashCode> ByHashCode;
    }

    readonly struct ByHashCode : IComparable<ByHashCode>
    {
        public readonly uint NodeId;
        public readonly int HashCode;

        public ByHashCode()
        {
            NodeId = NodeStruct.NodeId_NULL;
            HashCode = int.MaxValue;
        }

        public ByHashCode(uint nodeId, int hash)
        {
            NodeId = nodeId;
            HashCode = hash;
        }

        public int CompareTo(ByHashCode other) => HashCode.CompareTo(other.HashCode);

        public override int GetHashCode() => HashCode;
    }

    public NodeHeap(int blockSize = 100_000)
    {
        this.blockSize = blockSize;
        current = new Block()
        {
            ByNodeId = new NodeStruct[blockSize],
            ByHashCode = new(blockSize)
        };
        heapBlocks.Add(current);
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

        ref var node = ref current.ByNodeId[curr];
        node.Reset();
        node.SetNodeId(curr);
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

        // Insert into sorted List `current.ByHashCode`
        var byHash = new ByHashCode(node.NodeId, node.HashCode);
        var idx = current.ByHashCode.BinarySearch(byHash);
        if (idx < 0) idx = ~idx;
        current.ByHashCode.Insert(idx, byHash);
    }

    public ref NodeStruct GetById(uint id)
    {
        if (id == NodeStruct.NodeId_NULL) throw new InvalidDataException(nameof(NodeStruct.NodeId_NULL));
        if (id == NodeStruct.NodeId_NonPooled) throw new InvalidDataException(nameof(NodeStruct.NodeId_NonPooled));
        return ref current.ByNodeId[id];
    }

    public bool TryGetByHashCode(ref NodeStruct find, out uint matchNodeId)
    {
        var matchIdx = current.ByHashCode.BinarySearch(new ByHashCode(NodeStruct.NodeId_NULL, find.HashCode));
        if (matchIdx < 0)    // not found
        {
            matchNodeId = NodeStruct.NodeId_NULL;
            return false;
        }

        var idx = matchIdx;
        while(idx < current.ByHashCode.Count )
        {
            var curr = current.ByHashCode[idx];
            if (curr.HashCode != find.HashCode) break;
            if (curr.NodeId != find.NodeId)
            {
                // possible match
                ref var nn = ref GetById(curr.NodeId);
                if(nn.IsMatch(ref find))
                {
                    matchNodeId = curr.NodeId;
                    return true;
                }
            }
            idx++;

        }


        matchNodeId = NodeStruct.NodeId_NULL;
        return false;

        for(int cc=0; cc<next; cc++)    // lock?
        {
            ref var curr = ref current.ByNodeId[cc];
            if (curr.HashCode == find.HashCode)
            {
            }
        }
        matchNodeId = NodeStruct.NodeId_NULL;
        return false;

    }

}


