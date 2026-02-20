using System.Diagnostics.CodeAnalysis;

namespace SokoSolve.LargeSearchSolver.Lookup;

public interface ILNodeLookup
{
    bool IsThreadSafe { get; }
    INodeHeap Heap { get; }
    bool TryFind(ref NodeStruct find, out uint matchNodeId);
    void Add(ref NodeStruct node);
}

public interface ILNodeLookupSelfCheck
{
    void Check();
}

public readonly struct NodeIndex(uint nodeId, int hashCode)
{
    public uint NodeId { get; } = nodeId;
    public int HashCode { get; } = hashCode;

    public override string ToString() => $"NodeId: {NodeId}, HashCode: {HashCode}";

    public override int GetHashCode() => HashCode;
} 
