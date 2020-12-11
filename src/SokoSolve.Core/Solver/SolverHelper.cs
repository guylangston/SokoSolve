using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Schema;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using TextRenderZ;
using VectorInt;
using VectorInt.Collections;
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
        /// </summary>
        public const int VersionUniversal = 005;

        public const string VersionUniversalText = "SolverNodeLookup using IExtendedFunctionalityDescriptor and chaining";

        public static string Describe(ISolver solver)
        {
            return $"v{solver.VersionMajor}.{solver.VersionMinor}u{solver.VersionUniversal} [{solver.GetType().Name}] {solver.VersionDescription}";
        }

        public static T Init<T>(T res, SolverCommand command) where T : SolverState
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            if (command.ExitConditions == null) throw new NullReferenceException(nameof(command.ExitConditions));
            if (command.Puzzle == null) throw new NullReferenceException(nameof(command.Puzzle));
            
            if (!command.Puzzle.IsValid(out string err))
            {
                throw new InvalidDataException($"Not a valid puzzle: {err}");
            }

            res.Statistics.Started = DateTime.Now;
            res.StaticMaps         = new StaticAnalysisMaps(command.Puzzle);
            res.SolutionsNodes     = new List<SolverNode>();
            return res;
        }


        public static Path? CheckSolutionChain(SolverState state, SolverNode fwdNode, SolverNode revNode)
        {
            var walls = state.Command.Puzzle.ToMap(state.Command.Puzzle.Definition.Wall);
            
            var rev = ConvertReverseNodeToPath(state.Command.Puzzle, revNode, walls);
            if (rev == null) return null;
            
            var bridge = PathFinder.Find(walls.BitwiseOR(fwdNode.CrateMap), fwdNode.PlayerAfter, revNode.PlayerAfter);
            if (bridge == null) return null; // invalid chain
            
            var fwd = ConvertForwardNodeToPath(fwdNode, walls);
            if (fwd == null) return null;

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

        public static Path? ConvertReverseNodeToPath(Puzzle puzzle, SolverNode node, IBitmap walls)
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
            var b     = walls.BitwiseOR(puzzle.ToMap(puzzle.Definition.AllCrates));
            var f     = puzzle.Player.Position;
            var t     = pathToRoot[0].PlayerAfter;
            var first = PathFinder.Find(b, f, t);
            if (first == null) return null;
            r.AddRange(first);

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
            var nodePerSec = (double) state.Statistics.TotalNodes / state.Statistics.DurationInSec;

            sb.Append(state.Exit.ToString().PadRight(20));

            if (state.Exception != null)
            {
                var ex = state.Exception is AggregateException agg
                    ? agg.InnerExceptions.First()
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
            
            sb.Append($" {state.Statistics.TotalNodes,12:#,##0} nodes at {nodePerSec,6:#,##0}/s in {state.Statistics.Elapsed.Humanize()}." );
            if (state.HasSolution)
            {
                var d = state.SolutionsNodes != null ? state.SolutionsNodes.Count : 0;
                var r = state.SolutionsChains != null ? state.SolutionsChains.Count : 0;
                    
                sb.AppendFormat(" {0} solutions.", d + r);
            }


            return sb.ToString();
        }

      

        public static bool CheckSolution(Puzzle puzzle, Path path, out string? desc)
        {
            if (path == null)
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
            var           game    = new SokobanGameLogic(puzzle);
            var           lastPos = game.Current.Player;
            StringBuilder sb      = new StringBuilder();
            var           nodes   = new List<SolverNodeDTO>();
            var           cc      = 0;
            var           pushes  = 0;

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

            FloodFill.Fill( fillConstraints, pp, output);
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

        public static List<DepthLineItem> ReportDepth(SolverNode root)
        {
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
        }
        
        
       
    }
}