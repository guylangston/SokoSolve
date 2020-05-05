using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using SokoSolve.Core;
using SokoSolve.Core.Components;
using SokoSolve.Core.Primitives;
using SokoSolve.Core.Solver;
using VectorInt;
using Xunit;
using Xunit.Abstractions;

namespace SokoSolve.Tests
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


        [Fact]
        public void WholeTree()
        {
            var exit = new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(1),
                StopOnSolution = true,
                TotalNodes     = int.MaxValue,
                TotalDead      = int.MaxValue
            };
            var command = new SolverCommand
            {
                Puzzle         = Puzzle.Builder.DefaultTestPuzzle(),
                Report         = TextWriter.Null,
                ExitConditions = exit
            };

            // act 
            var solver = new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial());
            var result = solver.Init(command);
            solver.Solve(result);
            result.ThrowErrors();

            var allNodes = ((SolverBaseState) result).Root.Recurse().ToArray();
            
            var mem    = new MemoryStream();
            var writer = new BinaryNodeSerializer();
            using (var sw = new BinaryWriter(mem, Encoding.Unicode, true))
            {
                writer.Write(sw, allNodes);
            }
            
            outp.WriteLine($"Memory Stream Size = {allNodes.Length}nodes => {mem.Length}b");
            
            mem.Seek(0, SeekOrigin.Begin);
            
            using (var sr = new BinaryReader(mem))
            {

                var all = writer.ReadAll(sr).ToArray();
                
                Assert.Equal(allNodes.Length, all.Length);
            }

        }

        [Fact]
        public void Assemble()
        {
            var exit = new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(1),
                StopOnSolution = true,
                TotalNodes     = int.MaxValue,
                TotalDead      = int.MaxValue
            };
            var command = new SolverCommand
            {
                Puzzle         = Puzzle.Builder.DefaultTestPuzzle(),
                Report         = TextWriter.Null,
                ExitConditions = exit
            };

            // act 
            var solver = new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial());
            var result = solver.Init(command);
            solver.Solve(result);
            result.ThrowErrors();

            var allNodes = ((SolverBaseState) result).Root.Recurse().ToArray();
            
            var mem    = new MemoryStream();
            var writer = new BinaryNodeSerializer();
            using (var sw = new BinaryWriter(mem, Encoding.Unicode, true))
            {
                writer.Write(sw, allNodes);
            }
            
            outp.WriteLine($"Memory Stream Size = {allNodes.Length}nodes => {mem.Length}b");
            
            mem.Seek(0, SeekOrigin.Begin);
            
            using (var sr = new BinaryReader(mem))
            {
                var t = writer.AssembleTree(sr);
            }
            
            

        }
        
        [Fact]
        public void WriteDefaultForwardSolution()
        {
            var exit = new ExitConditions
            {
                Duration       = TimeSpan.FromSeconds(10),
                StopOnSolution = true,
                TotalNodes     = int.MaxValue,
                TotalDead      = int.MaxValue
            };
            var command = new SolverCommand
            {
                Puzzle         = Puzzle.Builder.DefaultTestPuzzle(),
                Report         = TextWriter.Null,
                ExitConditions = exit
            };

            // act 
            var solver = new SingleThreadedForwardSolver(new SolverNodeFactoryTrivial());
            var result = solver.Init(command);
            solver.Solve(result);
            result.ThrowErrors();
            Assert.True(result.HasSolution);

            var root = ((SolverBaseState) result).Root;

            using (var f = File.Create(Path.Combine(TestHelper.GetDataPath(), "./SavedState/default.ssbn")))
            {
                var writer = new BinaryNodeSerializer();
                writer.WriteTree(new BinaryWriter(f), root);
            }
        }

        [Fact]
        public void KeyClash()
        {
            var p = Path.Combine(TestHelper.GetDataPath(), "./SavedState/default.ssbn");
            if (!File.Exists(p))
            {
                outp.WriteLine("Skipped: not file");
                return;
            }
            
            using (var f = File.OpenRead(Path.Combine(TestHelper.GetDataPath(), "./SavedState/default.ssbn")))
            {
                var writer = new BinaryNodeSerializer();
                var nodes = writer.ReadAll(new BinaryReader(f));

                Dictionary<int, int> hash = new Dictionary<int, int>();
                foreach (var n in nodes)
                {
                    if (hash.TryGetValue(n.HashCode, out var c))
                    {
                        hash[n.HashCode] = c + 1;
                    }
                    else
                    {
                        hash[n.HashCode] = 1;
                    }
                }

                foreach (var pair in hash.OrderByDescending(x=>x.Value).Take(20))
                {
                    outp.WriteLine(pair.ToString());
                }
            }

        }


    }
}