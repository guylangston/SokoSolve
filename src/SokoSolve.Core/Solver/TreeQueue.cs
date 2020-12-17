using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SokoSolve.Core.Debugger;

namespace SokoSolve.Core.Solver
{
    public class TreeQueue : ISolverQueue
    {
        private readonly List<Band> depthBands;
        private readonly IDebugEventPublisher report;

        private int count;

        public TreeQueue(IEnumerable<SolverNode>? start, IDebugEventPublisher report)
        {
            Statistics = new SolverStatistics();
            depthBands = new List<Band>();
            this.report = report;
            count = 0;
            if (start != null)
                foreach (var ss in start)
                    Enqueue(ss);
        }

        public TreeQueue(SolverNode start, IDebugEventPublisher report) : this(new[] {start}, report)
        {
        }

        public TreeQueue(IDebugEventPublisher report) : this((IEnumerable<SolverNode>) null!, report)
        {
        }

        public bool IsEmpty => count == 0;

        public SolverStatistics Statistics { get; }
        public string TypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => ImmutableArray<(string name, string text)>.Empty;


        public void Init(SolverState state) {}
        
        public void Enqueue(SolverNode node)
        {
            var d = node.GetDepth();
            if (Statistics.DepthMax < d) Statistics.DepthMax = d;
            while (depthBands.Count <= d) depthBands.Add(new Band());
            depthBands[d].Add(node);
            depthBands[d].NodesProcessed++;
            count++;
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes) Enqueue(node);
        }
        
        public bool TrySample([MaybeNullWhen(false)] out SolverNode node)
        {
            node = null;
            return false; // not thread sage
        }

        public SolverNode? Dequeue()
        {
            foreach (var band in depthBands)
                if (band.Any())
                {
                    var q = band[band.Count - 1];
                    band.RemoveAt(band.Count - 1);
                    count--;


                    var depth = depthBands.IndexOf(band);
                    if (depth > Statistics.DepthCompleted) Statistics.DepthCompleted = depth;

                    if (report != null && band.Count == 0)
                        report.RaiseFormat(this, SolverDebug.DepthComplete, "Depth {0} exhausted {1} nodes.", depth,
                            band.NodesProcessed);
                    return q;
                }

            return null;
        }

        public IReadOnlyCollection<SolverNode>? Dequeue(int count)
        {
            foreach (var band in depthBands)
            {
                var l = band.Count;
                if (l > 0)
                {
                    if (l > count)
                    {
                        var r = new SolverNode[count];
                        band.CopyTo(0, r, 0, count);
                        band.RemoveRange(0, count);
                        this.count -= count;
                        return r;
                    }
                    else
                    {
                        var r = band.ToArray();
                        band.Clear();
                        this.count -= r.Length;

                        var depth = depthBands.IndexOf(band);
                        if (depth > Statistics.DepthCompleted) Statistics.DepthCompleted = depth;

                        if (report != null && band.Count == 0)
                            report.RaiseFormat(this, SolverDebug.DepthComplete, "Depth {0} exhausted {1} nodes.",
                                depthBands.IndexOf(band), band.NodesProcessed);
                        return r;
                    }
                }
            }

            return null;
        }

        private class Band : List<SolverNode>
        {
            public int NodesProcessed { get; set; }
        }

        
    }
}