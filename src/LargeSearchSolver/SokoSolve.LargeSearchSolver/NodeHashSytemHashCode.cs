namespace SokoSolve.LargeSearchSolver;

public class NodeHashSytemHashCode : INodeHashCalculator
{
    public NodeHashSytemHashCode(NSContext context)
    {
        Context = context;
    }

    public bool IsStable => false;
    public NSContext Context { get; }

    public int Calculate(ref NodeStruct node)
    {
        var hash = new HashCode();
        hash.AddBytes(node.GetBufferCrateMap());
        hash.AddBytes(node.GetBufferMoveMap());
        return hash.ToHashCode();
    }
}




