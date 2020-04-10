using System.Collections.Generic;
using SokoSolve.Core.Primitives;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
{
    public class IntComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y) return 1;
            if (x < y) return -1;
            return 0;
        }
    }
    
    public class BinarySearchTreeTests
    {
        private readonly ITestOutputHelper outp;

        public BinarySearchTreeTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void CanConstruct()
        {
            var sample = new int[] {23, 5, 12, 55, 67, 34, 33};

            var bst = new BinarySearchTree<int>(new IntComparer());
            foreach (var val in sample)
            {
                var n = bst.Add(val);
            }
            
            Assert.Equal(23, bst.Root.Value);
            Assert.Equal(5,  bst.GetMin().Value);
            Assert.Equal(67, bst.GetMax().Value);
            Assert.Equal(sample.Length, bst.Count);

            // foreach (var node in bst)
            // {
            //     outp.WriteLine(node.ToString());
            // }
            
        }
        
    }
}