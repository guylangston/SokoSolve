using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Common;
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
        private ITestOutputHelper outp;

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
            
            Assert.Equal(sample.OrderBy(x=>x).ToArray(), bst.ToArray());
            
        }
        
        [Fact]
        public void CanConstruct_NonUnique()
        {
            var sample = new int[] {23, 5, 5,5, 33};

            var bst = new BinarySearchTree<int>(new IntComparer());
            foreach (var val in sample)
            {
                var n = bst.Add(val);
            }
            
            Assert.Equal(23, bst.Root.Value);
            Assert.Equal(5,  bst.GetMin().Value);
            Assert.Equal(33, bst.GetMax().Value);
            Assert.Equal(sample.Length, bst.Count);
            
            Assert.Equal(sample.OrderBy(x=>x).ToArray(), bst.ToArray());
        }

        [Fact]
        public void RenderToGraphVis()
        {
            var sample = new int[]{23, 5, 12, 55, 67, 34, 33};

            var bst = new BinarySearchTree<int>(new IntComparer());
            foreach (var val in sample)
            {
                var n = bst.Add(val);
            }
            
            var sb = new FluentStringBuilder();
            sb.AppendLine("digraph g {");
            sb.AppendLine("rankdir=TB;");
            foreach (var node in bst.GetNodes())
            {
                if (node.Left != null)    sb.AppendLine($"\t{node.Value}->{node.Left.Value}[label=\"L\"]");
                if (node.Right != null)   sb.AppendLine($"\t{node.Value}->{node.Right.Value}[label=\"R\"]");
               // if (node.Parent != null)  sb.AppendLine($"\t{node.Value}->{node.Parent.Value}");
            }

            sb.AppendLine("}");
            
            outp.WriteLine(sb);
        }
        
    }
}