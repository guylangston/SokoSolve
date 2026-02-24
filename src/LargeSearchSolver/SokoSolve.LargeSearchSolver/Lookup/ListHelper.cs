namespace SokoSolve.LargeSearchSolver.Lookup;

public static class ListHelper
{

    // TODO: Needs unit test
    public static int InsertSorted<T>(List<T> list, T newItem, IComparer<T> compare)
    {
        for(int cc=0; cc<list.Count; cc++)
        {
            var cmp = compare.Compare(list[cc], newItem);
            if (cmp >= 0)
            {
                list.Insert(cc, newItem);
                return cc;
            }
        }

        list.Add(newItem);
        return list.Count-1;
    }
    public static int InsertSorted<T>(List<T> list, T newItem, Comparison<T> compare)
    {
        for(int cc=0; cc<list.Count; cc++)
        {
            var cmp = compare(list[cc], newItem);
            if (cmp >= 0)
            {
                list.Insert(cc, newItem);
                return cc;
            }
        }

        list.Add(newItem);
        return list.Count-1;
    }
}



