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
        for(int cc=0; cc<Context.Height; cc++)
        {
            hash.Add(node.GetMapLineCrate(cc));
            hash.Add(node.GetMapLineMove(cc));
        }
        return hash.ToHashCode();
    }
}




