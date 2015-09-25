using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Sokoban.Core.Common;
using Sokoban.Core.Debugger;

namespace Sokoban.Core.Solver
{
    public class TreeQueue : ISolverQueue
    {
        private readonly IDebugEventPublisher report;
        private readonly List<Band> depthBands;
       
        private int count;

        public SolverStatistics Statistics { get; private set; }

        class Band : List<SolverNode>
        {
            public int NodesProcessed { get; set; }
        }

        public TreeQueue(IEnumerable<SolverNode> start, IDebugEventPublisher report)
        {
            Statistics = new SolverStatistics();
            depthBands = new List<Band>();
            this.report = report;
            count = 0;
            if (start != null)
            {
                foreach (var ss in start)
                {
                    Enqueue(ss);
                }    
            }
        }

        public TreeQueue(SolverNode start, IDebugEventPublisher report) : this(new SolverNode[] { start }, report) { }
        public TreeQueue(IDebugEventPublisher report) : this((IEnumerable<SolverNode>)null, report) { }

        public bool IsEmpty { get { return count == 0; } }


        public void Enqueue(SolverNode node)
        {
            var d = node.GetDepth();
            if (Statistics.DepthMax < d) Statistics.DepthMax = d;
            while (depthBands.Count <= d)
            {
                depthBands.Add(new Band());
            }
            depthBands[d].Add(node);
            depthBands[d].NodesProcessed++;
            count++;
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            foreach (var node in nodes)
            {
                Enqueue(node);
            }
        }

        public SolverNode Dequeue()
        {
            foreach (var band in depthBands)
            {
                if (band.Any())
                {
                    var q = band[band.Count-1];
                    band.RemoveAt(band.Count - 1);
                    count--;

                   
                    var depth = depthBands.IndexOf(band);
                    if (depth > Statistics.DepthCompleted)
                    {
                        Statistics.DepthCompleted = depth;
                    }
                    
                    if (report != null && band.Count == 0)
                    {
                        report.RaiseFormat(this, SolverDebug.DepthComplete, "Depth {0} exhausted {1} nodes.", depth, band.NodesProcessed);
                    }
                    return q;
                }
            }
            return null;
        }

        public SolverNode[] Dequeue(int count)
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
                        {
                           
                            report.RaiseFormat(this, SolverDebug.DepthComplete, "Depth {0} exhausted {1} nodes.", depthBands.IndexOf(band), band.NodesProcessed);
                        }
                        return r;
                    }
                    
                }
            }
            return null;
        }



    }

    public class ThreadSafeSolverQueueWrapper : ISolverQueue
    {
        private readonly ISolverQueue inner;
        

        public ThreadSafeSolverQueueWrapper(ISolverQueue inner)
        {
            this.inner = inner;
        }

        public SolverStatistics Statistics { get { return inner.Statistics; } }

        public void Enqueue(SolverNode node)
        {
            lock (this)
            {
                inner.Enqueue(node); 
            }
        }

        public void Enqueue(IEnumerable<SolverNode> nodes)
        {
            lock(this) 
            {
                inner.Enqueue(nodes);
            }
        }

        public SolverNode Dequeue()
        {
            lock(this)
            {
                return inner.Dequeue();
            }
        }

        public SolverNode[] Dequeue(int count)
        {
            lock(this)
            {
                return inner.Dequeue(count);
            }
        }
    }

}