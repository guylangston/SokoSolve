using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using TextRenderZ;
using VectorInt;
using Path = SokoSolve.Core.Analytics.Path;

namespace SokoSolve.Core.Solver
{
    public static class SolverHelper
    {
        /// <summary>
        ///     001 - Initial Version
        ///     002 - Turned on compiler optimisations
        ///     003 - Dropped global pool to forward and reverse pool; better segmenting
        ///     004 - Added faster SolverNodeLookupThreadSafeBuffer, SolverQueueConcurrent
        ///     005 - SolverNodeLookup using IExtendedFunctionalityDescriptor and chaining
        ///     006 - Refactored SolverState to be consistent
        ///     007 - Pool=Eval Queue=UnEval (so lookup needed for both per tree)
        /// </summary>
        public const int VersionUniversal = 007;

        public const string VersionUniversalText = "Pool=Eval Queue=UnEval (so lookup needed for both per tree)";

        public static string Describe(ISolver solver)
            => $"v{solver.VersionMajor}.{solver.VersionMinor}u{solver.VersionUniversal} [{solver.GetType().Name}] {solver.VersionDescription}";

        public static Path? CheckSolutionChain(SolverState state, SolverNode fwdNode, SolverNode revNode)
        {
            var walls = state.Command.Puzzle.ToMap(state.Command.Puzzle.Definition.Wall);

            var fwd = ConvertForwardNodeToPath(fwdNode, walls);
            if (fwd == null)
            {
                state.Command.Debug?.RaiseFormat(nameof(CheckSolutionChain), SolverDebug.FalseSolution, "Bad Chain", fwdNode, revNode, "fwd");
                return null;
            }

            var bridge = PathFinder.Find(walls.BitwiseOR(fwdNode.CrateMap), fwdNode.PlayerAfter, revNode.PlayerAfter);
            if (bridge == null)
            {
                state.Command.Debug?.RaiseFormat(nameof(CheckSolutionChain), SolverDebug.FalseSolution, "Bad Chain", fwdNode, revNode, "bridge");
                return null;
            }

            var rev = ConvertReverseNodeToPath(state.Command.Puzzle, revNode, walls, false);
            if (rev == null)
            {
                state.Command.Debug?.RaiseFormat(nameof(CheckSolutionChain), SolverDebug.FalseSolution, "Bad Chain", fwdNode, revNode, "rev");
                return null;
            }

            var p = new Path()
            {
                Description  = "Fwd↔Rev",
                NodeDepthFwd = fwdNode.GetDepth(),
                NodeDepthRev = revNode.GetDepth()
            };
            p.AddRange(fwd);
            p.AddRange(bridge);
            p.AddRange(rev);

            if (!CheckSolution(state.Command.Puzzle, p, out var error))
            {
                state.Command.Debug?.RaiseFormat(nameof(CheckSolutionChain), SolverDebug.FalseSolution, "Bad Chain", fwdNode, revNode, error);
                return null;
            }

            return p;

        }

        public static Path? ConvertForwardNodeToPath(SolverNode node, IBitmap walls)
        {
            var pathToRoot = node.PathToRoot();
            var offset = GeneralHelper.OffsetWalk(pathToRoot);
            var r = new Path()
            {
                Description = "Fwd",
                NodeDepthFwd = node.GetDepth()
            };
            foreach (var pair in offset)
            {
                var boundry = walls.BitwiseOR(pair.Item1.CrateMap);
                var start = pair.Item1.PlayerAfter;
                var end = pair.Item2.PlayerBefore;

                var walk = PathFinder.Find(boundry, start, end);
                if (walk == null) return null;
                r.AddRange(walk);
                r.Add(pair.Item2.PlayerAfter - pair.Item2.PlayerBefore);
            }

            return r;
        }

        public static Path? ConvertReverseNodeToPath(Puzzle puzzle, SolverNode node, IBitmap walls, bool includePuzzleStart)
        {
            var pathToRoot = node.PathToRoot().ToList();
            pathToRoot.Reverse();

            var offset = GeneralHelper.OffsetWalk(pathToRoot).ToList();

            var r = new Path()
            {
                Description  = "Rev",
                NodeDepthRev = pathToRoot.Count
            };
            var cc = 0;

            // PuzzleStart to First Push
            if (includePuzzleStart)
            {
                var b     = walls.BitwiseOR(puzzle.ToMap(puzzle.Definition.AllCrates));
                var f     = puzzle.Player.Position;
                var t     = pathToRoot[0].PlayerAfter;
                var first = PathFinder.Find(b, f, t);
                if (first == null) return null;
                r.AddRange(first);
            }

            foreach (var pair in offset)
            {
                if (pair.Item2.Parent == null) break;

                r.Add(pair.Item1.PlayerBefore - pair.Item1.PlayerAfter);
                var boundry = walls.BitwiseOR(pair.Item2.CrateMap);
                var start   = pair.Item1.PlayerBefore;
                var end     = pair.Item2.PlayerAfter;
                var walk    = PathFinder.Find(boundry, start, end);
                if (walk == null) return null;
                r.AddRange(walk);

                cc++;
            }

            if (pathToRoot.Count > 1)
            {
                var last = pathToRoot[pathToRoot.Count - 2];
                r.Add(last.PlayerBefore - last.PlayerAfter);
            }
            return r;
        }

        public static string GenerateSummary(SolverState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var sb = new StringBuilder();
            var nodePerSec = (double) state.GlobalStats.TotalNodes / state.GlobalStats.DurationInSec;

            sb.Append(state.Exit.ToString().PadRight(20));

            if (state.Exception != null)
            {
                var ex = state.Exception is AggregateException agg
                    ? agg.InnerExceptions.FirstOrDefault()
                    : state.Exception;
                if (ex is NullReferenceException)
                {
                    sb.Append("[NULL] " + StringUtil.ToLines(ex.StackTrace).First().TrimStart('\t').Trim());
                }
                else
                {
                    sb.Append(StringUtil.Truncate(StringUtil.StripLineFeeds($"[{ex.GetType().Name}] {ex.Message}"), 180));
                }
                return sb.ToString();
            }

            sb.Append($" {state.GlobalStats.TotalNodes,12:#,##0} nodes at {nodePerSec,6:#,##0}/s in {state.GlobalStats.Elapsed.Humanize()}." );
            if (state.HasSolution)
            {
                sb.Append(" SOLUTION ");
                var cc = 0;
                foreach (var sol in state.Solutions)
                {
                    if (cc > 0) sb.Append(" | ");
                    sb.Append(sol.ToStringSummary());
                    cc++;
                }
            }

            return sb.ToString();
        }

        public static bool CheckSolution(Puzzle puzzle, Path? path, out string? desc)
        {
            if (path is null)
            {
                desc = "Invalid path";
                return false;
            }

            var game = new SokobanGameLogic(puzzle);
            var cc = 0;
            foreach (var step in path)
            {
                var m = game.Move(step);
                if (m == MoveResult.Win)
                {
                    desc = null;
                    return true;
                }

                if (m != MoveResult.OkPush && m != MoveResult.OkStep)
                {
                    desc = $"Step #{cc}/{path.Count} dir:{step} move result was error={m}\n{game.Current}\nPath: {path.ToStringFull()}";
                    return false;
                }

                cc++;
            }

            desc = "Path complete; but it did not state in a Solution. Final Position was:\n" + game.Current;
            return false;
        }

        public static (int pushCount, string withPushes, SolverNodeDTO[] nodes) ConvertSolutionToNodes(Puzzle puzzle, Path path)
        {
            var game    = new SokobanGameLogic(puzzle);
            var lastPos = game.Current.Player;
            var sb      = new StringBuilder();
            var nodes   = new List<SolverNodeDTO>();
            var cc      = 0;
            var pushes  = 0;

            nodes.Add(CreateNodeDTO(game.Current));

            foreach (var step in path)
            {
                if (cc >= 40)
                {
                    sb.AppendLine();
                    cc = 0;
                }
                var m = game.Move(step);
                if (m == MoveResult.OkPush)
                {
                    nodes.Add(CreateNodeDTO(game.Current));

                    cc++;
                    pushes++;
                    sb.Append(Path.ToString(step).ToUpper());
                }
                else if (m == MoveResult.OkStep)
                {

                    cc++;
                    sb.Append(Path.ToString(step).ToLower());
                }
            }
            return (pushes, sb.ToString(), nodes.ToArray());
        }

        private static SolverNodeDTO CreateNodeDTO(Puzzle puzzle)
        {
            var crates  = puzzle.ToMap(puzzle.Definition.AllCrates);
            var moveMap = FloodFillUsingWallAndCrates(puzzle.ToMap(puzzle.Definition.Wall), crates, puzzle.Player.Position);
            return new SolverNodeDTO()
            {
                Puzzle = puzzle,
                CrateMap = crates,
                MoveMap = moveMap,
                Hash = SolverNode.CalcHashCode(crates, moveMap)
            };
        }

        public static Bitmap FloodFillUsingWallAndCrates(IBitmap wall, IBitmap crate, VectorInt2 pp)
        {
            var fillConstraints = new BitmapSpan(wall.Size, stackalloc uint[wall.Height]);
            fillConstraints.SetBitwiseOR(wall, crate);

            return FloodFill.Fill(fillConstraints, pp);
        }

        public static void FloodFillUsingWallAndCratesInline(IBitmap wall, IBitmap crate, VectorInt2 pp, IBitmap output)
        {
            var fillConstraints = new BitmapSpan(wall.Size, stackalloc uint[wall.Height]);
            fillConstraints.SetBitwiseOR(wall, crate);
            if (output is Bitmap bb)
            {
                FloodFill.FillRecursiveOptimised( fillConstraints, pp.X, pp.Y, bb);
            }
            else
            {
                FloodFill.Fill(fillConstraints, pp, output);
            }

        }

        public class DepthLineItem
        {
            public int Depth { get; set; }
            public float GrowthRate { get; set; }
            public int Total { get; set; }
            public int UnEval { get; set; }
            public int Closed { get; set; }
            public int Dups { get; set; }
            public SolverNode Last { get; set; }
            public SolverNode LastUnEval { get; set; }
        }

        public static Task<List<DepthLineItem>> ReportDepth(SolverNode root) => Task.Run(() => {
            var res = new List<DepthLineItem>();
            foreach (var n in root.Recurse())
            {
                var d = n.GetDepth();
                while (res.Count <= d)
                {
                    var dd = new DepthLineItem();
                    res.Add(dd);
                    dd.Depth = res.IndexOf(dd); // safer
                }

                var line = res[d];
                line.Total++;
                if (n.IsClosed) line.Closed++;
                if (n.Status == SolverNodeStatus.UnEval)
                {
                    line.UnEval++;
                    line.LastUnEval = n;
                }

                if (n.Status == SolverNodeStatus.Duplicate)
                {
                    line.Dups++;
                }

                line.Last = n;
            }

            foreach ((DepthLineItem a, DepthLineItem b)  in res.PairUp())
            {
                if (b != null)
                {
                    b.GrowthRate = (float)b.Total / (float) a.Total;
                }
            }

            return res;

        });

        class PathNode : ITreeNodeParent
        {
            public PathNode(PathNode parent, VectorInt2 position, VectorInt2 dir)
            {
                Parent    = parent;
                Position  = position;
                Direction = dir;
            }

            public PathNode?  Parent    { get; }
            public VectorInt2 Position  { get; }
            public VectorInt2 Direction { get; }
            public bool       Solution  { get; set; }

            ITreeNodeParent? ITreeNodeParent.Parent => Parent;
        }

        public static IEnumerable<VectorInt2> FindPath(IBitmap within, VectorInt2 start, VectorInt2 end)
        {
            var p    = new PathNode(null, start, VectorInt2.Zero);
            var done = new Bitmap(within.Size);

            var solutions = new List<PathNode>();

            Test(p);

            var sol = solutions.OrderBy(x => x.GetDepth()).FirstOrDefault();
            if (sol != null)
            {
                return sol.PathToRoot().Select(x => x.Direction).Where(x=>x.IsUnit);
            }
            return null;

            void Test(PathNode node)
            {
                node.Solution       = (node.Position == end);
                if (node.Solution)
                {
                    solutions.Add(node);
                }
                done[node.Position] = true;

                VectorInt2 t, tt;

                tt = VectorInt2.Up;
                t  = node.Position + tt;
                if (done.Contains(t) && !done[t] && within[t]) Test(new PathNode(node, t, tt));

                tt = VectorInt2.Down;
                t  = node.Position + tt;
                if (done.Contains(t) && !done[t] && within[t]) Test(new PathNode(node, t, tt));

                tt = VectorInt2.Left;
                t  = node.Position + tt;
                if (done.Contains(t) && !done[t] && within[t]) Test(new PathNode(node, t, tt));

                tt = VectorInt2.Right;
                t  = node.Position + tt;
                if (done.Contains(t) && !done[t] && within[t]) Test(new PathNode(node, t, tt));
            }
        }
    }
}
