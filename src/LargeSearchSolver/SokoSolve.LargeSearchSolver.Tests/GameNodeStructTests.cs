using SokoSolve.Core.Lib;
using SokoSolve.LargeSearchSolver.GameLogic;
using VectorInt;

namespace SokoSolve.LargeSearchSolver.Tests;

public class GameNodeStructTests
{
    [Fact]
    public void CanPlaySQ1P1()
    {
        var p = Primitives.Puzzle.Builder.FromLines(TestLibrary.Default.Puzzle.ToStringList());
        var sol = TestLibrary.Default.Solution.ToString();

        var st = new SokoSolve.Primitives.Analytics.StaticAnalysisMaps(p);
        ISokobanGame game = new GameNodeStruct(st, p.Player.Position, new NodeHashCalculator());

        Assert.NotEmpty(sol);
        SokobanMoveResult r = SokobanMoveResult.None;
        foreach(var c in sol)
        {
            if (!char.IsAsciiLetter(c)) continue;
            var d = new Direction(char.ToUpper(c));
            r = game.Move(d);
            Assert.NotEqual(SokobanMoveResult.Invalid, r);
        }
        Assert.Equal(SokobanMoveResult.Solution, r);

    }

    [Fact]
    public void CanPlaySQ1P1_WithNodes()
    {
        var p = Primitives.Puzzle.Builder.FromLines(TestLibrary.Default.Puzzle.ToStringList());
        var sol = TestLibrary.Default.Solution.ToString();

        var st = new SokoSolve.Primitives.Analytics.StaticAnalysisMaps(p);
        var game = new GameNodeStruct(st, p.Player.Position, new NodeHashCalculator());

        Assert.NotEmpty(sol);
        SokobanMoveResult r = SokobanMoveResult.None;
        foreach(var c in sol)
        {
            if (!char.IsAsciiLetter(c)) continue;
            var d = new Direction(char.ToUpper(c));
            var move = game.Move(d);
            r = move.Result;
            if (move.Push != null)
            {
                // Console.WriteLine(move.Push.Value.ToDebugString());
            }
            Assert.NotEqual(SokobanMoveResult.Invalid, r);
        }
        Assert.Equal(SokobanMoveResult.Solution, r);

    }
}
