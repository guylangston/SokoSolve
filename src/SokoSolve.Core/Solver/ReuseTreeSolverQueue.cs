using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Solver
{
    public class ReuseTreeSolverQueue : ISolverQueue
    {
        private SolverNode root;

        public ReuseTreeSolverQueue(SolverNode root)
        {
            this.root = root;
        }

        public SolverNode Root
        {
            get => root;
            set => root = value ?? throw new ArgumentNullException(nameof(value));
        }

        public SolverStatistics Statistics { get; } = new SolverStatistics();

        public string TypeDescriptor => "Reuse the tree";
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => ImmutableArray<(string name, string text)>.Empty;

        public void Enqueue(SolverNode node){ }

        public void Enqueue(IEnumerable<SolverNode> nodes) { }

        public SolverNode? Dequeue()
        {
            SolverNode uneval = null;
            while (uneval == null || uneval.Status != SolverNodeStatus.UnEval)
            {
                uneval = root.RecursiveAll().FirstOrDefault(x => x.Status == SolverNodeStatus.UnEval);
            }

            return uneval;
        }

        public IReadOnlyCollection<SolverNode>? Dequeue(int count)
        {
            var res = new SolverNode[count];

            for (int i = 0; i < count; i++)
            {
                res[i] = Dequeue();
            }

            return res;
        }
    }
}