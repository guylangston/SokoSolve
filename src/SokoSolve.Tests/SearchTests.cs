using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.Diagnostics.Tracing.Parsers.JSDumpHeap;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

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
        
        
        [Benchmark]
        public BinarySearchTree<int> BinarySearchTreeInner()
        {
            var list = new BinarySearchTree<int>(new IntComparer());
            foreach (var i in data)
            {
                list.Add(i);   
            }

            return list;
        }
        
        [Fact]
        public void BinarySearchTree()
        {
            var list = LinkedListInner();
            
            Assert.True(ListHelper.IsSorted(list.ToArray(), (a, b) => a.CompareTo(b)));
        }
    }


    public class ScratchTests
    {
        private ITestOutputHelper outp;

        public ScratchTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void ExpInCSharp()
        {
            var x = 1d;
            var e = Enumerable.Range(0, 100).Sum(n =>  Math.Pow(x, n) / Factorial((int)n));
            outp.WriteLine(e.ToString());

        }
        
        double Exp(double x) =>  Enumerable.Range(0, 100).Sum(n =>  Math.Pow(x, n) / Factorial((int)n));

        private double Factorial(int d)
        {
            double r = 1;
            for (var x = 2; x <= d; x++)
            {
                r *= (double)x;
            }

            return r;
        }
    }


}