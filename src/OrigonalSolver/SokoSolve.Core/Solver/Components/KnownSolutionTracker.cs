using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Lib.DB;

namespace SokoSolve.Core.Solver.Components
{
    public interface IKnownSolutionTracker
    {
        void Init(SolverState state);
        void EvalComplete(SolverState state, TreeState tree, SolverNode evalComplete);

        string                                 Status      { get; }
        ImmutableDictionary<int, TrackedNode>? NodeLookup  { get; }
        public TrackedNode?                    LastForward { get; }
        public TrackedNode?                    LastReverse { get; }
    }

    public class TrackedNode
    {
        public int           Depth    { get; set; }
        public SolverNodeDTO Node     { get; set; }
        public SolverNode?   Found    { get; set; }
        public TimeSpan?     Elapsed { get; set; }
    }

    public class KnownSolutionTracker : IKnownSolutionTracker
    {

        private readonly ISokobanSolutionRepository repo;
        private readonly int solutionId;
        private IReadOnlyCollection<SolutionDTO>? solutions;
        private SolutionDTO? solution;

        public KnownSolutionTracker(ISokobanSolutionRepository repo, int solutionId)
        {
            this.repo       = repo;
            this.solutionId = solutionId;
        }

        public string?                                Status      { get; set; }
        public SolverNodeDTO[]?                       Nodes       { get; set; }
        public ImmutableDictionary<int, TrackedNode>? NodeLookup  { get; set; }
        public TrackedNode?                           LastForward { get; set; }
        public TrackedNode?                           LastReverse { get; set; }

        public void Init(SolverState state)
        {
            this.solutions = repo.GetPuzzleSolutions(state.Command.PuzzleIdent);
            if (this.solutions is {Count: 0}) return;

            this.solution  = solutionId <= 0 ? solutions.First()
                :  solutions.First(x => x.SolutionId == solutionId);
            if (solution.Path == null) throw new Exception("Invalid Solution Path");

            var result = SolverHelper.ConvertSolutionToNodes(state.Command.Puzzle, new Path(solution.Path!));
            this.Nodes = result.nodes;

            this.NodeLookup = Nodes.WithIndex().ToImmutableDictionary(x => x.item.Hash, x=>new TrackedNode()
            {
                Depth = x.index,
                Node = x.item
            });
        }

        public void EvalComplete(SolverState state, TreeState tree, SolverNode evalComplete)
        {
            Check(state, evalComplete);
            if (evalComplete.HasChildren)
            {
                foreach (var kid in evalComplete.Children)
                {
                    Check(state, kid);
                }
            }

        }
        private void Check(SolverState solverState, SolverNode node)
        {
            if (this.Nodes == null) return;

            if (NodeLookup!.TryGetValue(node.GetHashCode(), out var track))
            {
                if (track.Found == null)
                {
                    if (track.Node.Equals(node))
                    {
                        track.Found   = node;
                        track.Elapsed = solverState.GlobalStats.Elapsed;

                        var count = NodeLookup.Count(x=>x.Value.Found != null);
                        var total = Nodes.Length;
                        var depth = 0;
                        if (node.Evaluator is ForwardEvaluator)
                        {
                            depth       = track.Depth;
                            LastForward = track;
                        }
                        else // rev
                        {
                            depth       = Nodes.Length - track.Depth;
                            LastReverse = track;
                        }

                        var s = $"[KnownSolution] {count,3}/{total,3} ({count*100f/total,3:0}%), progress:R{LastForward?.Depth}:R{(Nodes.Length - LastReverse?.Depth)}," +
                                $" [#{track.Node.Hash}:d={depth}/{node.Evaluator.TypeDescriptor}]";

                        Status = s;
                    }
                }
            }
        }

    }
}
