namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupSharding : ILNodeLookup, ILNodeLookupNested, ISolverComponent
{
    readonly ILNodeLookup[] shards;

    public LNodeLookupSharding(INodeHeap heap, NSContext context, int shardCount, Func<INodeHeap, NSContext, ILNodeLookup> shardFactory)
    {
        Heap = heap;
        Context = context;
        shards = new ILNodeLookup[shardCount];
        for(int cc=0; cc<shardCount; cc++)
        {
            shards[cc] = shardFactory(heap, context);
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
    public NSContext Context { get; }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        var shardIdx = Math.Abs(find.HashCode % shards.Length);
        return shards[shardIdx].TryFind(ref find, out matchNodeId);
    }

    public void Add(ref NodeStruct node)
    {
        var shardIdx = Math.Abs(node.HashCode % shards.Length); // Abs(hashcode) directly may give 2s-complement overflow 
        shards[shardIdx].Add(ref node);
    }

    public IEnumerable<(string Desc, ILNodeLookup Inner)> GetNested()
    {
        for(var cc=0; cc<shards.Length; cc++)
        {
            yield return ($"Shard[{cc}]", shards[cc] );
        }
    }
}


