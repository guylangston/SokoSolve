namespace SokoSolve.LargeSearchSolver;

public class NodeHashSytemHashCode : INodeHashCalculator
{
    public bool IsStable => false;

    public int Calculate(ref NodeStruct node)
    {
        var hash = new HashCode();
        for(int cc=0; cc<node.Height; cc++)
        {
            hash.Add(node.GetMapLineCrate(cc));
            hash.Add(node.GetMapLineMove(cc));
        }
        return hash.ToHashCode();
    }
}




