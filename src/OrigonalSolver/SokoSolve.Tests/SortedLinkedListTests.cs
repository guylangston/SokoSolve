using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Common;
using Xunit;

namespace SokoSolve.Tests
{
    public class SortedLinkedListTests
    {
        [Fact]
        public void CanInsert()
        {
            var unsorted = new[] {3, 12, 3, 5, 6, 55, 2, 5, 6, 8, 4, 11, 9, 4};
            var sorted = unsorted.OrderBy(x => x).ToArray();

            var linked = new LinkedList<int>();
            foreach (var x in unsorted)
            {
                linked.InsertSorted(x, (a, b) => a.CompareTo(b));
            }

            Assert.Equal(sorted, linked.ToArray());
        }

    }
}
