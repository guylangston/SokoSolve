using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public static class SolverHelper
    {
        /// <summary>
        ///     001 - Initial Version
        ///     002 - Turned on compiler optimisations
        ///     003 - Droped global pool to forward and reverse pool; better segmenting
        ///     004 - Added faster SolverNodeLookupThreadSafeBuffer, SolverQueueConcurrent
        /// </summary>
        public const int VersionUniversal = 004;

        public const string VersionUniversalText = "Droped global pool to forward and reverse pool; better segmenting";

        public static string Describe(ISolver solver)
        {
            return string.Format("v{0}.{1}u{2} ({3}) {4} -- {5}",
                solver.VersionMajor, solver.VersionMinor, solver.VersionUniversal, solver.GetType().Name,
                solver.VersionDescription, VersionUniversalText);
        }

        public static T Init<T>(T res, SolverCommand command) where T : SolverCommandResult
        {
            if (command.ExitConditions == null) throw new NullReferenceException();

            res.Command = command;
            res.Statistics = new SolverStatistics
            {
                Started = DateTime.Now
            };
            res.StaticMaps = StaticAnalysis.Generate(command.Puzzle);
            res.SolutionsNodes = new List<SolverNode>();
            res.StaticMaps.DeadMap = DeadMapAnalysis.FindDeadMap(res.StaticMaps);
            return res;
        }

        public static SolverNode CreateRoot(Puzzle puzzle)
        {
            var crate = puzzle.ToMap(puzzle.Definition.AllCrates);
            var moveBoundry = crate.BitwiseOR(puzzle.ToMap(puzzle.Definition.Wall));
            var move = FloodFill.Fill(moveBoundry, puzzle.Player.Position);
            var root = new SolverNode(
                puzzle.Player.Position, puzzle.Player.Position,
                new VectorInt2(), new VectorInt2(),
                crate, move,
                -1,
                null
            );
            
            return root;
        }


        public static void GetSolutions(SolverCommandResult state, bool check)
        {
            var walls = state.Command.Puzzle.ToMap(state.Command.Puzzle.Definition.Wall);
            
            
            state.Solutions = new List<Path>();
            
            if (state.SolutionsNodes != null)
            {
                state.Solutions.AddRange(state.SolutionsNodes.Select(x => ConvertSolutionNodeToPath(x, walls, state.Command.Puzzle)));
            }
                
            if (state.SolutionsNodesReverse != null)
            {
                foreach (var tuple in state.SolutionsNodesReverse)
                {
                    var rev = ConvertReverseNodeToPath(tuple.ReverseNode, walls);
                    var fwd = ConvertForwardNodeToPath(tuple.ForwardNode, walls);
                    var bridge = PathFinder.Find(walls.BitwiseOR(tuple.ForwardNode.CrateMap),
                        tuple.ForwardNode.PlayerAfter, tuple.ReverseNode.PlayerAfter);

                    //Console.WriteLine("Forward Leg: {0}", fwd);
                    //Console.WriteLine("Reverse Leg: {0}", rev);
                    //Console.WriteLine("Bridge {0} => {1}", tuple.ForwardNode.ToStringDebugPositions(),
                    //    tuple.ReverseNode.ToStringDebugPositions());

                    var p = new Path();
                    p.AddRange(fwd);
                    p.AddRange(bridge);
                    p.AddRange(rev);
                    state.Solutions.Add(p);
                }
            }

            if (check && state.Solutions.Any())
            {
                int cc = 0;
                foreach (var pathSolution in state.Solutions.ToArray())
                {
                    cc++;
                    if (!CheckSolution(state.Command.Puzzle, pathSolution, out var error))
                    {
                        state.SolutionsInvalid ??= new List<(Path, string)>();
                        state.SolutionsInvalid.Add((pathSolution, error));
                        state.Solutions.Remove(pathSolution);

                        if (state.Command.Report != null)
                        {
                            state.Command.Report.WriteLine($"Solution #{cc++} [{(check ? "Valid" : "INVALID!" + error)}] =>\n{pathSolution}");
                        }
                    }
                }
            }
            
            
        }

        public static Path ConvertSolutionNodeToPath(SolverNode node, IBitmap walls, Puzzle puzzle)
        {
            if (node.Evaluator.GetType() == typeof(ReverseEvaluator))
            {
                var pathToRoot = node.PathToRoot();
                pathToRoot.Reverse();

                var offset = GeneralHelper.OffsetWalk(pathToRoot).ToList();


                var r = new Path();
                var cc = 0;

                // PuzzleStart to First Push
                var b = walls.BitwiseOR(puzzle.ToMap(puzzle.Definition.AllCrates));
                var f = puzzle.Player.Position;
                var t = pathToRoot[0].PlayerAfter;
                var first = PathFinder.Find(b, f, t);
                if (first == null)
                    //throw new Exception(string.Format("Bad Path at INIT. {0} => {1}. This is an indicator or a FALSE positive. Ie. An invalid start position.\n{2}", f, t, b)); // Not solution
                    return null;
                r.AddRange(first);

                foreach (var pair in offset)
                {
                    if (pair.Item2.Parent == null) break;

                    r.Add(pair.Item1.PlayerBefore - pair.Item1.PlayerAfter);
                    var boundry = walls.BitwiseOR(pair.Item2.CrateMap);
                    var start   = pair.Item1.PlayerBefore;
                    var end     = pair.Item2.PlayerAfter;
                    var walk    = PathFinder.Find(boundry, start, end);
                    if (walk == null) throw new Exception($"Bad Path at step {cc}\n"); // Not solution
                    r.AddRange(walk);

                    //Console.WriteLine("PAIR: {0}", cc);
                    //Console.WriteLine("{0} => {1}", pair.Item1.PlayerBefore, pair.Item1.PlayerAfter);
                    //Console.WriteLine(pair.Item1.ToStringDebug());
                    //Console.WriteLine("{0} => {1}", pair.Item2.PlayerBefore, pair.Item2.PlayerAfter);
                    //Console.WriteLine(pair.Item2.ToStringDebug());

                    //var debug = boundry.ToCharMap();
                    //debug[start] = 'S';
                    //debug[end] = 'E';
                    //var d = debug.ToString();
                    //Console.WriteLine(d);
                    //Console.WriteLine(r);
                    cc++;
                }

                if (pathToRoot.Count > 1)
                {
                    var last = pathToRoot[pathToRoot.Count - 2];
                    r.Add(last.PlayerBefore - last.PlayerAfter);
                }


                return r;
            }

            return ConvertForwardNodeToPath(node, walls);
        }

        public static Path ConvertForwardNodeToPath(SolverNode node, IBitmap walls)
        {
            var pathToRoot = node.PathToRoot();
            var offset = GeneralHelper.OffsetWalk(pathToRoot);
            var r = new Path();
            foreach (var pair in offset)
            {
                var boundry = walls.BitwiseOR(pair.Item1.CrateMap);
                var start = pair.Item1.PlayerAfter;
                var end = pair.Item2.PlayerBefore;

                var walk = PathFinder.Find(boundry, start, end);
                if (walk == null) throw new Exception("bad Path\n"); // Not solution
                r.AddRange(walk);
                r.Add(pair.Item2.PlayerAfter - pair.Item2.PlayerBefore);
            }

            return r;
        }

        public static Path ConvertReverseNodeToPath(SolverNode node, IBitmap walls)
        {
            var pathToRoot = node.PathToRoot();
            pathToRoot.Reverse();

            //// debug
            //if (true)
            //{
            //    foreach (var d in pathToRoot)
            //    {
            //        Console.WriteLine(d.ToStringDebugPositions());
            //        Console.WriteLine(d.ToStringDebug());
            //        Console.WriteLine("--------------------------");

            //    }
            //}

            pathToRoot.RemoveAt(pathToRoot.Count - 1);

            var offset = GeneralHelper.OffsetWalk(pathToRoot);
            var r = new Path();
            var cc = 0;
            foreach (var pair in offset)
            {
                r.Add(pair.Item1.PlayerBefore - pair.Item1.PlayerAfter);

                var boundry = walls.BitwiseOR(pair.Item2.CrateMap);
                var start = pair.Item1.PlayerBefore;
                var end = pair.Item2.PlayerAfter;

                var walk = PathFinder.Find(boundry, start, end);
                if (walk == null)
                    throw new Exception(string.Format("Bad Path at {0}, path={1}", cc, r)); // Not solution
                r.AddRange(walk);
                cc++;
            }

            if (pathToRoot.Count > 1)
            {
                var last = pathToRoot[pathToRoot.Count - 1];
                r.Add(last.CrateBefore - last.CrateAfter);
            }

            return r;
        }


        public static string GenerateSummary(SolverCommandResult state)
        {
            if (state == null) throw new ArgumentNullException("state");

            var sb = new StringBuilder();
            var nodePerSec = (double) state.Statistics.TotalNodes / state.Statistics.DurationInSec;
          
            if (state.EarlyExit)
            {
                sb.Append("Stopped. ");
                if (state.Exception != null)
                {
                    sb.Append(state.Exception.Message);
                }
                else
                {
                    sb.Append(state.Exit);
                    sb.Append(" => ");
                    sb.Append(state.Statistics);
                }
            }
            else
            {
                
                if (state.HasSolution)
                {
                    sb.Append("SUCCESS. ");
                    var d = state.SolutionsNodes != null ? state.SolutionsNodes.Count : 0;
                    var r = state.SolutionsNodesReverse != null ? state.SolutionsNodesReverse.Count : 0;
                    
                    sb.AppendFormat("{0} solutions.", d + r);
                }
                else
                {
                    sb.Append("Failed. ");
                    sb.Append(state.Exit.ToString());
                    sb.Append(".");
                }

                if (state.SolutionsInvalid != null && state.SolutionsInvalid.Count > 0)
                {
                    sb.Append(" !INVALID SOLUTIONS!");
                }
                    
            }
            sb.Append($" {state.Statistics.TotalNodes:#,##0} nodes at {nodePerSec:#,##0}/s in {state.Statistics.Elapased.Humanize()}." );

            return sb.ToString();
        }


        public static bool CheckSolution(Puzzle puzzle, Path path, out string desc)
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

                if (m != MoveResult.Ok)
                {
                    desc = $"Move #{cc} of {path.Count} dir:{step} result not OK, but was {m}\n{game.Current}";
                    return false;
                }

                cc++;
            }

            desc = "Path complete; but it did not result in a Solution. Final Position was:\n" + game.Current;
            return false;
        }
    }
}