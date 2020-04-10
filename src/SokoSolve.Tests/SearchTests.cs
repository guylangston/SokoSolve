using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using SokoSolve.Core.Common;
using Xunit;

namespace SokoSolve.Tests
{


    public class SearchTests
    {
        private int[] data;
        
        public SearchTests()
        {
            var r = new Random(1234); // we want the same seq
            this.data = Enumerable.Range(0, 20000).Select(x => r.Next()).ToArray();
        }
        
        // Closely adapted from SolverPoolSortedList
        static void InsertSorted(List<int> list, int node)
        {
            var cc = 0;
            while (cc < list.Count && list[cc].GetHashCode() < node.GetHashCode()) 
                cc++;
            list.Insert(cc, node);
        }
        
        [Benchmark]
        public List<int> SolverPoolSortedListInner()
        {
            var list = new List<int>();
            foreach (var i in data)
            {
                InsertSorted(list, i);   
            }
            return list;
        }
        [Fact]
        public void SolverPoolSortedList()
        {
            var list = SolverPoolSortedListInner();
            
            Assert.True(ListHelper.IsSorted(list, (a, b) => a.CompareTo(b)));
        }
        
        
        [Benchmark]
        public LinkedList<int> LinkedListInner()
        {
            var list = new LinkedList<int>();
            foreach (var i in data)
            {
                list.InsertSorted(i, (a, b) => a.CompareTo(b));   
            }

            return list;
        }
        
        [Fact]
        public void LinkedList()
        {
            var list = LinkedListInner();
            
            Assert.True(ListHelper.IsSorted(list.ToArray(), (a, b) => a.CompareTo(b)));
        }
    }

   
}