namespace SokoSolve.LargeSearchSolver;

public class NodeHashCalculator : INodeHashCalculator
{
    public int Calculate(ref NodeStruct node)
    {
        unchecked
        {
            uint hash = 17;
            for(int cc=0; cc<node.Height; cc++)
            {
                hash = hash * 31u + node.GetMapLineCrate(cc);
                hash = hash * 31u + node.GetMapLineMove(cc);
            }
            return (int)hash;
        }
    }
}



