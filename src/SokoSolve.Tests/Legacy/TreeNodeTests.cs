using System.Linq;
using SokoSolve.Core.Common;
using Xunit;

namespace SokoSolve.Tests.Legacy
{
    public class TestNode : TreeNodeBase
    {
        public TestNode(string id)
        {
            Id = id;
        }

        public TestNode()
        {
        }

        public string Id { get; set; }

        public override string ToString()
        {
            return Id;
        }
    }


    public class TreeNodeTests
    {
        [Xunit.Fact]
        public void PathToRoot()
        {
            var tree = new TestNode("A")
            {
                new TestNode("A.A")
                {
                    new TestNode("A.A.A"),
                    new TestNode("A.A.B")
                }
            };

            var report = new TestReport();

            foreach (var node in tree.Last().PathToRoot()) report.WriteLine(node);

            var expected =
                @"A
A.A
A.A.B
";
            Assert.Equal(new TestReport(expected), report);
        }

        [Xunit.Fact]
        public void ResursiveEnumeration()
        {
            var tree = new TestNode("A")
            {
                new TestNode("A.A")
                {
                    new TestNode("A.A.A"),
                    new TestNode("A.A.B")
                    {
                        new TestNode("A.A.B.A"),
                        new TestNode("A.A.B.B")
                    }
                },
                new TestNode("A.B")
            };


            var report = new TestReport();

            foreach (var n in tree) report.WriteLine(n);


            var expected =
                @"A
A.A
A.A.A
A.A.B
A.A.B.A
A.A.B.B
A.B
";
            Assert.Equal(new TestReport(expected), report);
        }

        [Xunit.Fact]
        public void ResursiveWhere()
        {
            var tree = new TestNode("A")
            {
                new TestNode("A.A")
                {
                    new TestNode("A.A.A"),
                    new TestNode("A.A.B")
                },
                new TestNode("A.B")
            };

            var report = new TestReport();

            foreach (var node in tree.Where(x => x.Id.StartsWith("A.A")))
                report.WriteLine("{0} Depth: {1}, Count: {2}", node, node.GetDepth(), node.Count(x => true));

            var expected =
                @"A.A Depth: 1, Count: 3
A.A.A Depth: 2, Count: 1
A.A.B Depth: 2, Count: 1
";
            Assert.Equal(new TestReport(expected), report);
        }
    }
}