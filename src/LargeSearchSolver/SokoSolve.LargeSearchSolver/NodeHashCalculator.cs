namespace SokoSolve.LargeSearchSolver;

public class NodeHashCalculator : INodeHashCalculator
{
    public NodeHashCalculator(NSContext context)
    {
        Context = context;
    }

    public bool IsStable => true;
    public NSContext Context { get; }

    public int Calculate(ref NodeStruct node)
    {
        unchecked
        {
            uint hash = 17;
            foreach(var b in node.GetBufferMoveMap())
            {
                hash = (hash * 31u) + (uint)b;
            }
            foreach(var b in node.GetBufferCrateMap())
            {
                hash = (hash * 31u) + (uint)b;
            }
            return (int)hash;
        }
    }
}



