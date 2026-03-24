namespace SokoSolve.LargeSearchSolver;

public interface ISolverComponent
{
    string GetComponentName();
    string Describe();
}

public interface ICoreSolverComponent : ISolverComponent
{
    bool IsThreadSafe { get; }
}

public interface INodeHeap : ICoreSolverComponent
{
    int Count { get; }

    ref NodeStruct Lease(); // thread-safe
    void Return(uint nodeId);
    void Commit(ref NodeStruct node); // makes not immutable

    ref NodeStruct GetById(uint nodeId);   // throw if not found

    /// <summary>Not threadsafe (expected to be used for unit tests only)</summary>
    IEnumerable<uint> EnumerateNodeIds { get; }
}

public interface INodeBacklog : ICoreSolverComponent
{
    int Count { get; }
    bool TryPop(out uint nextNodeId);
    void Push(IEnumerable<uint> newItems);
}

public interface ILNodeStructEvaluator : ICoreSolverComponent
{
    uint InitRoot(LSolverState state);
    void Evaluate(LSolverState state, ref NodeStruct node);
}

public interface INodeHashCalculator
{
    /// <summary>Stable meaning same result across machines / proceses </summary>
    bool IsStable { get; }
    NSContext Context { get; }
    int Calculate(ref NodeStruct node);
}

public interface ILNodeLookup : ICoreSolverComponent
{
    INodeHeap Heap { get; }
    NSContext Context { get; }
    bool TryFind(ref NodeStruct find, out uint matchNodeId);
    void Add(ref NodeStruct node);
}
