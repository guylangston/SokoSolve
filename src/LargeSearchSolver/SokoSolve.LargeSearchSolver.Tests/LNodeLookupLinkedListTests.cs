using SokoSolve.LargeSearchSolver.Lookup;
namespace SokoSolve.LargeSearchSolver.Tests;

public class LNodeLookupLinkedListTests
{

    [Fact]
    public void Standard()
    {
        StandardTest(new LNodeLookupLinkedList(new NodeHeap(100)));
    }

    public static void StandardTest(ILNodeLookup lookup)
    {
        int size = 10;

        // Add items
        var buffer = new NodeStruct[size];
        for(int cc=0; cc<size; cc++)
        {
            ref var item = ref lookup.Heap.Lease();
            item.SetMapSize(NodeStruct.MaxMapWidth, NodeStruct.MaxMapHeight);
            item.SetCrateMapAt((byte)(cc % 32), (byte)(cc % NodeStruct.MaxMapHeight), true);
            item.SetMoveMapAt((byte)(cc % 32), (byte)(cc % NodeStruct.MaxMapHeight), true);

            if (cc < size-2)
            {
                item.SetHashCode(cc + 100_000);
            }
            else
            {
                // collision
                item.SetHashCode(99_999);
            }

            buffer[cc] = item;  // COPY (which is great for this test)
            lookup.Add(ref item);
        }

        // Confirm that lookup is sorted?
        if (lookup is ILNodeLookupSelfCheck check)
        {
            check.Check();
        }


        // Confirm we can get them back
        for(int cc=0; cc<size; cc++)
        {
            var match = lookup.TryFind(ref buffer[cc], out var id);
            Assert.True(match);
            Assert.True(NodeStruct.IsValidId(id));
            Assert.Equal(buffer[cc].NodeId, id);
        }

        var notExist = new NodeStruct();
        Assert.False(lookup.TryFind(ref notExist, out _));

        notExist.SetHashCode(1_000_000);
        Assert.False(lookup.TryFind(ref notExist, out _));

    }
}


