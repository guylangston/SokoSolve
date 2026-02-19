namespace SokoSolve.LargeSearchSolver;

public class NodeBacklog : INodeBacklog
{
    Queue<uint> items =  new();

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

    public int Count => items.Count;

    public IEnumerable<uint> Peek() => items;
}


