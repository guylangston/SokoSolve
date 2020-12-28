using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Lib.DB;

namespace SokoSolve.Core.Solver.Components
{
    public interface IKnownSolutionTracker
    {
        void Init(SolverState state);
        void EvalComplete(SolverState state, TreeState tree, SolverNode evalComplete);
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
        
        public SolverNodeDTO[]                       Nodes      { get; set; }
        public ImmutableDictionary<int, TrackedNode> NodeLookup { get; set; }

        public class TrackedNode
        {
            public SolverNodeDTO Node  { get; set; }
            public SolverNode?   Found { get; set; }
        }

        public void Init(SolverState state)
        {
            this.solutions = repo.GetPuzzleSolutions(state.Command.PuzzleIdent);
            this.solution  = solutions.First(x => x.SolutionId == solutionId);
            if (solution.Path == null) throw new Exception("Invalid Solution Path");
            
            var result = SolverHelper.ConvertSolutionToNodes(state.Command.Puzzle, new Path(solution.Path!));
            this.Nodes = result.nodes;

            this.NodeLookup = Nodes.ToImmutableDictionary(x => x.Hash, x=>new TrackedNode()
            {
                Node = x
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
            if (NodeLookup.TryGetValue(node.GetHashCode(), out var track))
            {
                if (track.Found == null)
                {
                    track.Found = node;
                    var count       = NodeLookup.Count(x=>x.Value.Found != null);
                    var total = Nodes.Length;
                    var s           = $"[TrackedSolutionNode] {count,3}/{total,3} ({count*100f/total,3:0}%)  [#{track.Node.Hash,12} / {node.Evaluator.TypeDescriptor}]";
                    
                    solverState.Command.Report?.WriteLine(s);
                    
                    Console.WriteLine();
                    Console.WriteLine(s);
                }
            }
        }
        
        



    }
}