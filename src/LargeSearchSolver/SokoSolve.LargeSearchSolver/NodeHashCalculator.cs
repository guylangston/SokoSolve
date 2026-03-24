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
            for(int cc=0; cc<Context.Height; cc++)
            {
                hash = hash * 31u + node.GetMapLineCrate(cc);
                hash = hash * 31u + node.GetMapLineMove(cc);
            }
            return (int)hash;
        }
    }
}



