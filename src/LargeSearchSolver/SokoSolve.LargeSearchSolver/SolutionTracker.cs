using System.Diagnostics;
using SokoSolve.LargeSearchSolver.GameLogic;
using SokoSolve.LargeSearchSolver.Lookup;
using VectorInt;

namespace SokoSolve.LargeSearchSolver;

public class SolutionTracker : INodeWatcher, ISolverComponent
{
    private NodeHeap? isolatedHeap;
    private LNodeLookupBlackRedTree? isolatedLookup;
    List<uint> foundFwd = new();
    List<uint> foundRev = new();

    public string Describe()=> "";
    public string GetComponentName() => nameof(SolutionTracker);

    public void Init(LSolverState state)
    {
        List<NodeStruct> pushes = new();
        var game = new GameNodeStruct(state.StaticMaps, state.Request.Puzzle.Player.Position, state.HashCalculator);
        foreach(var step in state.Request.TrackSolution!)
        {
            if (!char.IsAsciiLetter(step)) continue;
            var d = new Direction(char.ToUpper(step));
            var move = game.Move(d);
            if (move.Push != null)
            {
                pushes.Add(move.Push.Value);
            }
        }

        var pushesArray = pushes.ToArray();
        this.isolatedHeap = new NodeHeap(pushes.Count);
        this.isolatedLookup = new LNodeLookupBlackRedTree(isolatedHeap);
        for (int cc=0; cc<pushes.Count; cc++)
        {
            ref var n = ref isolatedHeap.Lease();
            n.SetFromNode(ref pushesArray[cc]);
            isolatedLookup.Add(ref n);
        }
    }

    public void OnCommit(ref NodeStruct node)
    {
        if (isolatedLookup!.TryFind(ref node, out var matchId))
        {
            if (node.Type == NodeStruct.NodeType_Forward)
            {
                if (!foundFwd.Contains(matchId))
                {
                    foundFwd.Add(matchId);
                }
            }
            else
            {
                if (!foundRev.Contains(matchId))
                {
                    foundRev.Add(matchId);
                }
            }
        }
    }

    public override string ToString()
    {
        var size = isolatedHeap!.Count;
        var found = foundFwd.Count + foundRev.Count;
        return $"TrackSol={foundFwd.Count}+{foundRev.Count}/{size}({found*100/size}%)";
    }

}

