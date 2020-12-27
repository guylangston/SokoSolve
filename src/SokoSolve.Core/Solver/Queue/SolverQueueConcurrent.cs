using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver.Queue
{
    // public class SolverQueueConcurrent : ISolverQueue
    // {
    //     private readonly ConcurrentQueue<SolverNode> queue = new ConcurrentQueue<SolverNode>();
    //     
    //     public SolverStatistics Statistics { get; } = new SolverStatistics();
    //     public string TypeDescriptor => GetType().Name;
    //     public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => null;
    //
    //     public int Count => queue.Count;
    //     
    //     public void Init(SolverState state) {}
    //     
    //     public bool TrySample(out SolverNode? node) => queue.TryPeek(out node);
    //     
    //     public void Enqueue(SolverNode node)
    //     {
    //         queue.Enqueue(node);
    //         Statistics.TotalNodes++;
    //         Statistics.CurrentNodes++;
    //     }
    //
    //     public void Enqueue(IEnumerable<SolverNode> nodes)
    //     {
    //         foreach (var n in nodes)
    //         {
    //             Enqueue(n);
    //         }
    //     }
    //
    //     public virtual SolverNode? Dequeue()
    //     {
    //         if (queue.TryDequeue(out var r))
    //         {
    //             Statistics.CurrentNodes--;
    //             return r;
    //         }
    //         return null;
    //     }
    //
    //     public virtual IReadOnlyCollection<SolverNode>? Dequeue(int count)
    //     {
    //         var l = new List<SolverNode>();
    //         while (l.Count < count && queue.TryDequeue(out var r))
    //         {
    //             Statistics.CurrentNodes--;
    //             l.Add(r);
    //         }
    //
    //         return l.ToArray();
    //     }
    // }

    // public class SolverQueueBreadthFirst : ISolverQueue
    // {
    //     private  SolverNode? root;
    //
    //     public SolverStatistics Statistics     { get; } = new SolverStatistics();
    //     public string           TypeDescriptor => GetType().Name;
    //     public IEnumerable<(string name, string text)>? GetTypeDescriptorProps(SolverState state) => null;
    //
    //     public void Init(SolverState state)
    //     {
    //         // Forward or Reverse?
    //     }
    //     
    //     public void Enqueue(SolverNode node)
    //     {
    //         Statistics.TotalNodes++;
    //         if (node.Parent == null) root = node;
    //     }
    //     public void Enqueue(IEnumerable<SolverNode> nodes)
    //     {
    //         foreach (var node in nodes)
    //         {
    //             Enqueue(node);
    //         }
    //     }
    //
    //     // TODO: Very slow and inefficient
    //     public SolverNode? Dequeue() 
    //         => root!.RecursiveAll().FirstOrDefault(x => x.Status == SolverNodeStatus.UnEval);
    //
    //     // TODO: Very slow and inefficient
    //     public IReadOnlyCollection<SolverNode>? Dequeue(int count)
    //         => root!.RecursiveAll().Where(x=>x.Status == SolverNodeStatus.UnEval)
    //                .Take(count)
    //                .ToArray();
    //
    //
    // }
}