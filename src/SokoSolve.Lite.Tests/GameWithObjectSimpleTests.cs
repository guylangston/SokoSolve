using System.Linq;
using Xunit;

namespace SokoSolve.Lite.Tests
{
    public class GameWithObjectSimpleTests
    {
        [Fact]
        public void PlayGame()
        {
            var gm = new GameWithObjectSimple(MapBuilder.Default);
            
            
            gm.Init();
            
            var path = MapBuilder.DefaultSolution;
            var p    = Util.ToPath(path).ToArray();
            
            foreach (var move in p[..^1])
            {
                var r = gm.Move(move);
                Assert.Equal(MoveResult.Ok, r);
            }
            Assert.Equal(MoveResult.Win, gm.Move(p[^1]));

            foreach (var goal in gm.Current.EachBlock().Where(x=>x.b == Block.Goal))
            {
                var hasCrate = gm.Get(Block.Crate, goal.p);
            }

        }
    }
}