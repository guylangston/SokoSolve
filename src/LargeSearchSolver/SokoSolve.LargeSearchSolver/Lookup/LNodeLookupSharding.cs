namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupSharding : ILNodeLookup, ISolverComponent
{
    readonly ILNodeLookup[] shards;

    public LNodeLookupSharding(INodeHeap heap, int shardCount, Func<INodeHeap, ILNodeLookup> shardFactory)
    {
        Heap = heap;
        shards = new ILNodeLookup[shardCount];
        for(int cc=0; cc<shardCount; cc++)
        {
            shards[cc] = shardFactory(heap);
        }
    }

    public string GetComponentName() => nameof(LNodeLookupCompound);
    public string Describe()
    {
        var inner = shards[0] is ISolverComponent s0 ? s0.Describe() : shards[0].GetType().Name;
        return $"Sharing[{shards.Length}] -> {inner}";
    }

    public bool IsThreadSafe => false;
    public INodeHeap Heap { get;  }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        var shardIdx = Math.Abs(find.HashCode) % shards.Length;
        return shards[shardIdx].TryFind(ref find, out matchNodeId);
    }

    public void Add(ref NodeStruct node)
    {
        var shardIdx = Math.Abs(node.HashCode) % shards.Length;
        shards[shardIdx].Add(ref node);
    }

}


