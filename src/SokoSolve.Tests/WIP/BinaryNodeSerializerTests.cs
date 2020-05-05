using System.IO;
using System.Text;
using SokoSolve.Core;
using SokoSolve.Core.Components;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using VectorInt;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests.WIP
{
    public class BinaryNodeSerializerTests
    {
        private ITestOutputHelper outp;

        public BinaryNodeSerializerTests(ITestOutputHelper outp)
        {
            this.outp = outp;
        }

        [Fact]
        public void SingleNode()
        {
            var d = Puzzle.Builder.DefaultTestPuzzle();
            
            var n = new SolverNode(null, d.Player.Position, VectorInt2.Left, d.ToMap(d.Definition.AllCrates),  d.ToMap(d.Definition.AllFloors));
            
            var mem    = new MemoryStream();
            var writer = new BinaryNodeSerializer();
            
            using (var sw = new BinaryWriter(mem, Encoding.Unicode, true))
            {
            
                writer.Write(sw, n);
            }

            
            mem.Seek(0, SeekOrigin.Begin);
            

            
            using (var sr = new BinaryReader(mem))
            {
                var temp = writer.Read(sr);
                
                Assert.Equal(n.SolverNodeId, temp.SolverNodeId);
                Assert.Equal(0, temp.ParentId);
                Assert.Equal(n.PlayerBefore.X, temp.PlayerBeforeX);
                Assert.Equal(n.PlayerBefore.Y, temp.PlayerBeforeY);
                Assert.Equal(n.Push.X, temp.PushX);
                Assert.Equal(n.Push.Y, temp.PushY);
                Assert.Equal(n.Status, (SolverNodeStatus)temp.Status);
                
                var c = n.CrateMap is BitmapByteSeq bs ? bs : new BitmapByteSeq(n.CrateMap);
                Assert.Equal(c.GetArray(), temp.Crate);
                
                var m = n.MoveMap is BitmapByteSeq ms ? ms : new BitmapByteSeq(n.MoveMap);
                Assert.Equal(m.GetArray(), temp.Move);
                
            }
        }

        [Fact]
        public void Header()
        {
            var mem    = new MemoryStream();
            var writer = new BinaryNodeSerializer();
            
            var d = Puzzle.Builder.DefaultTestPuzzle();
            
            using (var sw = new BinaryWriter(mem, Encoding.Unicode, true))
            {
                writer.WriteHeader(sw, d.Size, 1234);
            }

            
            mem.Seek(0, SeekOrigin.Begin);
            

            
            using (var sr = new BinaryReader(mem))
            {

                var x = writer.ReadHeader(sr);
                Assert.Equal(d.Size, x.size);
                Assert.Equal(1234, x.count);

            }
        } 

        
    }
}