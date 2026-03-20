using System.Text;
using Xunit.Abstractions;

namespace SokoSolve.LargeSearchSolver.Tests;

public abstract class NodeStructTests
{
    protected readonly ITestOutputHelper testOutputHelper;

    protected NodeStructTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    protected void AssertNodeReportEqual(string expect, LSolverState state, IEnumerable<uint> nodes)
    {
        var sb = new StringBuilder();
        foreach(var nodeId in nodes)
        {
            ref var node = ref state.Heap.GetById(nodeId);
            sb.Append(node.ToDebugString(state));
        }
        var actual = sb.ToString();
        if (expect != actual)
        {
            testOutputHelper.WriteLine("ACTUAL vvvvvv");
            testOutputHelper.WriteLine(actual);
            testOutputHelper.WriteLine("ACTUAL ^^^^^^");
        }
        Assert.Equal(expect, actual, false,true, true, true);
    }

    protected class TestPeek : ISolverCoodinatorPeek
    {
        readonly Func<LSolverState, int, bool> funcPeek;

        public TestPeek(Func<LSolverState, int, bool> funcPeek)
        {
            this.funcPeek = funcPeek;
        }

        public int PeekEvery { get; set; } = 1;

        public void Finished() { }

        public bool TickUpdate(LSolverState state, int totalNodes, ref NodeStruct current) => funcPeek(state, totalNodes);
    }
}



