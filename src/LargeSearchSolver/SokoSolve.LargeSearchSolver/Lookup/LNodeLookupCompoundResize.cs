namespace SokoSolve.LargeSearchSolver.Lookup;

public class LNodeLookupCompoundResize : ILNodeLookup, ILNodeLookupNested, ISolverComponent
{
    readonly LNodeLookupBlackRedTree dynamicInitial;
    LNodeLookupImmutable? immutable = null;

    public LNodeLookupCompoundResize(INodeHeap heap)
    {
        Heap = heap;
        dynamicInitial = new LNodeLookupBlackRedTree(heap);
    }

    public string GetComponentName() => nameof(LNodeLookupCompoundResize);

    public string Describe() => $"Dynamic={dynamicInitial.GetType().Name}({ThresholdDynamic}), Immutable[{typeof(LNodeLookupImmutable).Name}]";
    public bool IsThreadSafe => false;
    public INodeHeap Heap { get;  }
    public int ThresholdDynamic { get; set; } = 1_000_000;

    public void Add(ref NodeStruct node)
    {
        dynamicInitial.Add(ref node);
        if (dynamicInitial.Count >= ThresholdDynamic)
        {
            // copy to immutable
            if (immutable == null)
            {
                var data = new NodeIndex[dynamicInitial.Count];
                dynamicInitial.CopyTo(data);
                immutable = new LNodeLookupImmutable(Heap, data);
                dynamicInitial.Clear();
                return;
            }
            else
            {
                var a = immutable.GetInnerArray();  // sorted
                var b = new NodeIndex[dynamicInitial.Count];
                dynamicInitial.CopyTo(b); // sorted
                dynamicInitial.Clear();

                var c = MergeUtils.MergeSorted(a, b);
                immutable = new LNodeLookupImmutable(Heap, c);
            }
        }
    }

    public bool TryFind(ref NodeStruct find, out uint matchNodeId)
    {
        if (dynamicInitial.TryFind(ref find, out var m1))
        {
            matchNodeId = m1;
            return true;
        }

        if (immutable != null)
        {
            return immutable.TryFind(ref find, out matchNodeId);
        }

        matchNodeId = NodeStruct.NodeId_NULL;
        return false;
    }

    public IEnumerable<(string Desc, ILNodeLookup Inner)> GetNested()
    {
        yield return ("Dynamic", dynamicInitial);
        if (immutable != null)
        {
            yield return ("Immutable+Resize", immutable);
        }
    }


    public static class MergeUtils
    {
        // Merges two sorted arrays into a new sorted array.
        public static T[] MergeSorted<T>(T[] a, T[] b) where T : IComparable<T>
        {
            var result = new T[a.Length + b.Length];
            int i = 0, j = 0, k = 0;

            while (i < a.Length && j < b.Length)
            {
                // Use <= to keep the merge stable (preserves order of equals from 'a' before 'b')
                if (a[i].CompareTo(b[j]) <= 0)
                    result[k++] = a[i++];
                else
                    result[k++] = b[j++];
            }

            // Copy any remainder (only one of these will actually copy anything)
            if (i < a.Length) Array.Copy(a, i, result, k, a.Length - i);
            else if (j < b.Length) Array.Copy(b, j, result, k, b.Length - j);

            return result;
        }

    }
}


