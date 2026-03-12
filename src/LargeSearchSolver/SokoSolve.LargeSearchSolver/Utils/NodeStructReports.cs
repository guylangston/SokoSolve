using System.Text;
using SokoSolve.Primitives;

namespace SokoSolve.LargeSearchSolver.Utils;

public static class NodeStructHelpers
{
    public static IEnumerable<uint> PathToRoot(INodeHeap heap, uint nodeid)
    {
        var n = nodeid;
        while(NodeStruct.IsValidId(n))
        {
            yield return n;
            ref var node = ref heap.GetById(n);
            n = node.ParentId;
        }
    }

    public static IEnumerable<uint> FindAllSolutionsNodes(LSolverState state)
    {
        // Get all solution nodes
        var sln = new List<uint>();
        foreach(var fwd in state.SolutionsForward)
        {
            sln.AddRange(NodeStructHelpers.PathToRoot(state.Heap, fwd));
        }
        foreach(var rev in state.SolutionsReverse)
        {
            sln.AddRange(NodeStructHelpers.PathToRoot(state.Heap, rev));
        }
        foreach(var pair in state.SolutionsChain)
        {
            sln.AddRange(NodeStructHelpers.PathToRoot(state.Heap, pair.chainForwardId));
            sln.AddRange(NodeStructHelpers.PathToRoot(state.Heap, pair.chainReverseId));
        }
        sln.Sort();
        return sln.Distinct();
    }
}

public static class NodeStructReports
{
    public static void RenderGraphVis(LSolverState state, TextWriter outf, IEnumerable<uint> nodes)
    {
        outf.WriteLine("digraph tree {");
        outf.WriteLine("\t rankdir=\"RL\";");
        foreach(var nodeId in nodes)
        {
            ref var node = ref state.Heap.GetById(nodeId);
            if (node.ParentId == NodeStruct.NodeId_NULL) continue;
            var nt = node.Type == NodeStruct.NodeType_Forward ? 'F' : 'R';
            var s = node.Status != NodeStatus.COMPLETE ? node.Status.ToString() : "";
            outf.WriteLine($"\t{node.NodeId} [ label=\"{nt}{node.NodeId}\n{s}\" ];");
        }
        foreach(var nodeId in nodes)
        {
            ref var node = ref state.Heap.GetById(nodeId);
            if (node.ParentId == NodeStruct.NodeId_NULL) continue;
            outf.WriteLine($"\t{node.NodeId} -> {node.ParentId};");
        }
        foreach(var p in state.SolutionsChain)
        {
            outf.WriteLine($"\t{p.chainForwardId} -> {p.chainReverseId}");
            outf.WriteLine($"\t{p.chainReverseId} -> {p.chainForwardId}");
        }
        outf.WriteLine("}");
    }

    public static void RenderTreeState(LSolverState state, TextWriter outf, IEnumerable<uint> nodes)
    {
        foreach(var nodeId in nodes)
        {
            WriteNodeLine(state, outf, nodeId);
        }

        static void WriteNodeLine(LSolverState state, TextWriter outf, uint nodeId)
        {
            ref var node = ref state.Heap.GetById(nodeId);
            var nt = node.Type == NodeStruct.NodeType_Forward ? 'F' : 'R';
            var crate = new Bitmap(state.Request.Puzzle.Size);
            node.CopyCrateMapTo(crate);
            var crateStr = EncodeBitmapToText(crate);
            var move = new Bitmap(state.Request.Puzzle.Size);
            node.CopyCrateMapTo(move);
            var moveStr = EncodeBitmapToText(move);

            outf.WriteLine($"{nt} {nodeId,10} {node.ParentId,10} {node.Status,-13} {node.HashCode,12} {crateStr} {moveStr}");
        }
    }

    /// <summary>Encode to a single line</summary>
    public static string EncodeBitmapToText(IBitmap map, bool includeSize = false)
    {
        var outf = new StringBuilder();
        if (includeSize)
        {
            outf.Append(map.Width);
            outf.Append(':');
            outf.Append(map.Height);
            outf.Append(':');
        }
        var bytes = BitmapHelper.ToBinarySequenceInBytes(map);
        return Convert.ToBase64String(bytes);
    }
}
