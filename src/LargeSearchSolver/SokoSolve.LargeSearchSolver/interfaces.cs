namespace SokoSolve.LargeSearchSolver;

// Q: What is the difference between the NodeHeap and the NodeTree
//
// Q: Is this threadsafe?

public interface INodeHeap
{
    ref NodeStruct LeaseNode();
    void ReturnLease(uint nodeId);

    ref NodeStruct GetById(uint nodeId);   // throw if not found
    bool TryGetByHashCode(out uint matchNodeId); // TODO: should this return a ref NodeStruct. Should this be in a different class?
}

public interface INodeBacklog
{
    bool TryPop(out uint nextNodeId);
    void Push(ref NodeStruct node);
}

// Partials/TODO placeholders
public class Puzzle { /* TODO */ }
public class StaticAnalysis { /* TODO */ }
