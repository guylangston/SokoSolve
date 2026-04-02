namespace SokoSolve.LargeSearchSolver;

public static class NodeStructTreeHelper
{
    public static bool TryGetPreviousSibling(ref NodeStruct c, INodeHeap heap, out uint prevNodeId)
    {
        if (c.ParentId == NodeStruct.NodeId_NULL)
        {
            prevNodeId = NodeStruct.NodeId_NULL;
            return false;
        }
        ref var parent = ref heap.GetById(c.ParentId);
        var sib = parent.FirstChildId;
        uint prev = NodeStruct.NodeId_NULL;
        while (sib != NodeStruct.NodeId_NULL)
        {
            if (sib == c.NodeId)
            {
                if (prev != NodeStruct.NodeId_NULL)
                {
                    prevNodeId = prev;
                    return true;
                }
                prevNodeId = NodeStruct.NodeId_NULL;
                return false;
            }
            prev = sib;
            sib = heap.GetById(sib).SiblingNextId;
        }
        prevNodeId = NodeStruct.NodeId_NULL;
        return false;
    }

    public static int GetDepth(ref NodeStruct node, INodeHeap heap)
    {
        ref var n = ref node;
        var depth = 0;
        while(n.ParentId != NodeStruct.NodeId_NULL)
        {
            n = ref heap.GetById(n.ParentId);
            depth++;
        }
        return depth;
    }

    public static int GetSiblingCount(ref NodeStruct node, INodeHeap heap)
    {
        if (node.ParentId == NodeStruct.NodeId_NULL) return 0;
        return GetChildCount(ref heap.GetById(node.ParentId), heap);
    }
    public static int GetChildCount(ref NodeStruct node, INodeHeap heap)
    {
        if (node.FirstChildId == NodeStruct.NodeId_NULL) return 0;
        var kid = node.FirstChildId;
        var count = 1;
        while (kid != NodeStruct.NodeId_NULL)
        {
            kid = heap.GetById(kid).SiblingNextId;
            count++;
        }
        return count;
    }


    public static int GetChildCountRecursive(ref NodeStruct node, INodeHeap heap)
    {
        if (node.FirstChildId == NodeStruct.NodeId_NULL) return 0;
        var kid = node.FirstChildId;
        var count = 0;
        while (kid != NodeStruct.NodeId_NULL)
        {
            ref var kidNode = ref heap.GetById(kid);
            count += GetChildCountRecursive(ref kidNode, heap) + 1;
            kid = kidNode.SiblingNextId;
        }
        return count;
    }

}



