using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Common;
using Xunit;

namespace SokoSolve.Tests
{
    public class BinarySearchTreeTests
    {
        private int[] data;
        
        public BinarySearchTreeTests()
        {
            var r = new Random(1234); // we want the same seq
            this.data = Enumerable.Range(0, 2000).Select(x => r.Next()).ToArray();
        }

        [Fact]
        public void SolverPoolSortedList()
        {
            var list = new List<int>();
            foreach (var i in data)
            {
                InsertSorted(list, i);   
            }
            
            Assert.True(ListHelper.IsSorted(list, (a, b) => a.CompareTo(b)));
        }
        
        [Fact]
        public void SolverPoolSortedList_LinqOrderd()
        {
            var list = new List<int>();
            foreach (var i in data.OrderBy(x=>x))
            {
                InsertSorted(list, i);   
            }
            Assert.True(ListHelper.IsSorted(list, (a, b) => a.CompareTo(b)));
        }
        
        // Closely adapted from SolverPoolSortedList
        static void InsertSorted(List<int> list, int node)
        {
            var cc = 0;
            while (cc < list.Count && list[cc].GetHashCode() < node.GetHashCode()) 
                cc++;
            list.Insert(cc, node);
        }
    }
}