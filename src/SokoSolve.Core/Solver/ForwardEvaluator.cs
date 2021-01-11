using System.Linq;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public class ForwardEvaluator : NodeEvaluator
    {
        public ForwardEvaluator(SolverCommand cmd, ISolverNodePoolingFactory nodePoolingFactory) : base(cmd, nodePoolingFactory)
        {
        }

        public SolverNodeRoot CreateRoot(Puzzle puzzle)
        {
            var crate       = puzzle.ToMap(puzzle.Definition.AllCrates);
            var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
            var move        = FloodFill.Fill(moveBoundry, puzzle.Player.Position);
            var root = new SolverNodeRoot(
                puzzle.Player.Position, new VectorInt2(),
                crate, move,
                this,
                puzzle
            );
            
            return root;
        }

        public override SolverNode Init(Puzzle puzzle, ISolverQueue queue)
        {
            var root = CreateRoot(puzzle);
            queue.Enqueue(root);
            return root;
        }

        protected override bool GenerateChildNodes(SolverState state, TreeStateCore tree, SolverNode node)
        {
            var solution = false;
            foreach (var move in node.MoveMap.TruePositions())
            {
                foreach (var dir in VectorInt2.Directions)
                {
                    var p   = move;
                    var pp  = p + dir;
                    var ppp = pp + dir;
                    if (node.CrateMap[pp]                                        // crate to push
                        && state.StaticMaps.FloorMap[ppp] && !node.CrateMap[ppp] // into free space?
                        && !state.StaticMaps.DeadMap[ppp])                       // Valid Push
                    {
                        var newKid = nodePoolingFactory.CreateFromPush(node, node.CrateMap, state.StaticMaps.WallMap, p, pp, ppp, dir);
                        if (EvaluateNewChild(state, tree, node, newKid))
                        {
                            solution = true;
                            if (state.Command.ExitConditions.StopOnSolution)
                            {
                                return true;// Are we ok to skip code below?    
                            }
                        }
                    }
                }
            }
            return solution;
        }
        
        protected override bool CheckAndBuildSingleTreeSolution(SolverState state, SolverNode newKid)
        {
            if (!newKid.IsSolutionForward(state.StaticMaps)) return false;
            
            var path = SolverHelper.ConvertForwardNodeToPath(newKid, state.StaticMaps.WallMap);
            if (path == null)
            {
                state.GlobalStats.Warnings++;
                state.Command.Debug?.Raise(this, SolverDebug.FalseSolution, newKid);
            }
            else
            {
                // Solution
                state.SolutionsNodes.Add(newKid);
                state.Solutions.Add(path);

                state.Command.Debug?.Raise(this, SolverDebug.Solution, newKid);

                foreach (var n in newKid.PathToRoot())
                    n.Status = SolverNodeStatus.SolutionPath;
                newKid.Status = SolverNodeStatus.Solution;

                return true;
            }
            return false;
        }
        
        protected override  bool CheckAndBuildSolutionChain(SolverStateDoubleTree state, SolverNode fwdNode, SolverNode revNode)
        {
            // Check solution
            var potential = SolverHelper.CheckSolutionChain(state, fwdNode, revNode); 
            if (potential != null)
            {
                foreach (var n in fwdNode.PathToRoot().Union(revNode.PathToRoot()))
                    n.Status = SolverNodeStatus.SolutionPath;
                fwdNode.Status = SolverNodeStatus.Solution;
                revNode.Status = SolverNodeStatus.Solution;
                
                var pair = new SolutionChain
                {
                    ForwardNode = fwdNode,
                    ReverseNode = revNode,
                    FoundUsing  = this,
                    Path = potential
                };
                state.SolutionsChains.Add(pair);

                
                state.Solutions.Add(potential);

                state.Command.Debug?.Raise(this, SolverDebug.Solution, pair);
                return true;
            }

            return false;
        }
    }
}