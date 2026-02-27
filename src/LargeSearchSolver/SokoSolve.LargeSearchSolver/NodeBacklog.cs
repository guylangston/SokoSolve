namespace SokoSolve.LargeSearchSolver;

public class NodeBacklog : INodeBacklog, ISolverComponent
{
    readonly Queue<uint> items;

    public const int SeedSize = 100_000;

    public NodeBacklog(int cap = SeedSize)
    {
        items = new(cap);
    }

    public string GetComponentName() => GetType().Name;
    public string Describe() => $"BlockSize: {SeedSize}";
    public int Count => items.Count;

    public void Push(IEnumerable<uint> newItems)
    {
        foreach(var id in newItems)
        {
            items.Enqueue(id);
        }
    }

    public bool TryPop(out uint nextNodeId)
    {
        if (items.Count > 0)
        {
            nextNodeId = items.Dequeue();
            return true;
        }

        nextNodeId = uint.MaxValue;
        return false;
    }

    public IEnumerable<uint> Peek() => items;
}


