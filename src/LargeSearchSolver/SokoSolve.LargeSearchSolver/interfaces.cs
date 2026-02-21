using System.Security.Cryptography;
using SokoSolve.Core.Primitives;

namespace SokoSolve.LargeSearchSolver;

// Q: What is the difference between the NodeHeap and the NodeTree
//
// Q: Is this threadsafe?

public interface INodeHeap
{
    int Count { get; }

    ref NodeStruct Lease(); // thread-safe
    void Return(uint nodeId);
    void Commit(ref NodeStruct node); // makes not immutable

    ref NodeStruct GetById(uint nodeId);   // throw if not found


}

public interface INodeBacklog
{
    int Count { get; }
    bool TryPop(out uint nextNodeId);
    void Push(IEnumerable<uint> newItems);
    //void Push(ref NodeStruct node);
}

public interface ILNodeStructEvaluator
{
    uint InitRoot(LSolverState state);
    void Evaluate(LSolverState state, ref NodeStruct node);
}

public interface INodeHashCalculator
{
    int Calculate(ref NodeStruct node);
}


