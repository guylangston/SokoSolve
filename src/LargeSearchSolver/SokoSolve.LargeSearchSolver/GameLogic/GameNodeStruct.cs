using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;
using VectorInt;

namespace SokoSolve.LargeSearchSolver.GameLogic;

public enum SokobanMoveResult { None, Invalid, OkStep, OkPush, Solution }

public interface ISokobanGame
{
    SokobanMoveResult Move(Direction d);
}

/// <summary>Implement the game logic targeting IBitmap rather than puzzle definitions</summary>
/// <remarks>The are lots of game implementations, each suited for specific tasks and with differrent dependancies</remarks>
public class GameNodeStruct : ISokobanGame
{
    readonly StaticMaps staticMaps;
    readonly VectorInt2 playerStart;
    readonly Bitmap crates;
    VectorInt2 playerCurr;
    readonly INodeHashCalculator hashCalc;

    public GameNodeStruct(StaticMaps staticMaps, VectorInt2 playerStart, INodeHashCalculator hashCalc)
    {
        this.staticMaps = staticMaps;
        this.playerCurr = this.playerStart = playerStart;
        this.crates = staticMaps.CrateStart.Clone();

        if (!staticMaps.FloorMap[playerCurr]) throw new InvalidDataException("Player must start on the floor");
        this.hashCalc = hashCalc;
    }


    public (SokobanMoveResult Result, NodeStruct? Push) Move(Direction move)
    {
        var pp = playerCurr + move;
        var ppp = pp + move;
        if (!staticMaps.FloorMap[pp]) 
        {
            return (SokobanMoveResult.Invalid, null); // must stop onto floor
        }

        if (!crates[pp]) // step
        {
            playerCurr = pp;
            return (SokobanMoveResult.OkStep, null);
        }
        else // push
        {
            if (staticMaps.FloorMap[ppp] && !crates[ppp]) // push into space
            {
                playerCurr = pp;
                crates[pp] = false;
                crates[ppp] = true;

                var moveBoundry = crates.BitwiseOR(staticMaps.WallMap);
                var moveMap        = FloodFill.Fill(moveBoundry, pp);

                var node = new NodeStruct();
                node.SetMapSize(crates.Width, crates.Height);
                node.SetStatus(NodeStatus.NONE);
                node.SetType(NodeStruct.NodeType_Forward);
                node.SetCrateMap(crates);
                node.SetMoveMap(moveMap);
                node.SetPlayer(pp.X, pp.Y);
                node.SetHashCode(hashCalc.Calculate(ref node));
                var moveDir = move.ToVectorInt2();
                node.SetPlayerPush(moveDir.X, moveDir.Y);

                // Solution?
                if (crates.BitwiseAND(staticMaps.GoalMap).Count >= crates.Count())
                {
                    return ( SokobanMoveResult.Solution, node );
                }
                return ( SokobanMoveResult.OkPush, node );
            }
            return (SokobanMoveResult.Invalid, null);
        }
    }

    SokobanMoveResult ISokobanGame.Move(Direction d)
    {
        var res = Move(d);
        return res.Result;
    }
}
