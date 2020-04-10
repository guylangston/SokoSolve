using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public enum SolverNodeStatus
    {
        UnEval,
        Evaluted,
        Dead,
        DeadRecursive,
        Duplicate,
        Evaluting,
        Solution,
        SolutionPath,
        InvalidSolution
    }
    
    public class SolverNode : TreeNodeBase, IStateMaps, IEquatable<IStateMaps>, IComparable<SolverNode>
    {
        private static readonly uint[] crateWeights = Primes.List.Select(x=>(uint)x).ToArray();
        private static readonly uint[] moveWeights = Primes.List.Skip(10).Select(x=>(uint)x).ToArray();
        private static volatile int nextId = 1;
        
        private readonly int hashCrate;
        private readonly int hashMove;
        private readonly int hash;
        
        public SolverNode(
            VectorInt2 playerBefore, VectorInt2 playerAfter, 
            VectorInt2 crateBefore, VectorInt2 crateAfter, 
            Bitmap crateMap, Bitmap moveMap,
            int goals,
            INodeEvaluator? evaluator)
        {
            PlayerBefore = playerBefore;
            PlayerAfter = playerAfter;
            CrateBefore = crateBefore;
            CrateAfter = crateAfter;
            CrateMap = crateMap;
            MoveMap = moveMap;
            Evaluator = evaluator;
            Goals = goals;
            Status = SolverNodeStatus.UnEval;

            SolverNodeId = Interlocked.Increment(ref nextId);

            unchecked
            {
                hashCrate = CrateMap.HashUsingWeights(crateWeights);
                hashMove  = MoveMap.HashUsingWeights(moveWeights);
                hash      = hashCrate ^ (hashMove << (MoveMap.Width / 2));
            }
        }

        public int               SolverNodeId { get; }
        public VectorInt2        PlayerBefore { get; }
        public VectorInt2        PlayerAfter  { get; }
        public VectorInt2        CrateBefore  { get; }
        public VectorInt2        CrateAfter   { get; }
        public Bitmap            CrateMap     { get; }
        public Bitmap            MoveMap      { get; }
        public INodeEvaluator?   Evaluator    { get; }
        public int               Goals        { get; }
        public SolverNodeStatus  Status       { get; set; }
        public List<SolverNode>? Duplicates   { get; set; }

        public new SolverNode? Parent => (SolverNode) base.Parent;
        public new IEnumerable<SolverNode>? Children => HasChildren 
            ? base.Children.Cast<SolverNode>() 
            : ImmutableArray<SolverNode>.Empty;

        public static readonly IComparer<SolverNode> ComparerInstance = new Comparer();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SolverNode other) => ComparerInstance.Compare(this, other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IStateMaps other) 
            => other != null && (CrateMap.Equals(other.CrateMap) && MoveMap.Equals(other.MoveMap));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => Equals((IStateMaps) obj);

        public override string ToString()
            => $"[Id:{SolverNodeId} #{GetHashCode()}] C{CrateMap.GetHashCode()} M{MoveMap.GetHashCode()} D{this.GetDepth()} {Status}";
      
        public string ToStringDebug()
        {
            var map = new Map<char>(CrateMap.Size);
            map.Fill('.');
            foreach (var c in CrateMap.TruePositions()) map[c] = '#';
            foreach (var m in MoveMap.TruePositions()) map[m] = 'p';
            return map.ToString();
        }
        
        public void AddDuplicate(SolverNode newKid)
        {
            if (Duplicates == null) Duplicates = new List<SolverNode>();
            Duplicates.Add(newKid);
        }

        public void CheckDead()
        {
            if (HasChildren && Children.All(x => x.Status == SolverNodeStatus.Dead || x.Status == SolverNodeStatus.DeadRecursive))
            {
                Status = SolverNodeStatus.DeadRecursive;
                Parent?.CheckDead();
            }
        }
        
        // TODO: Could be optimised? AND and COMPARE seems expensive
        public bool IsSolutionForward(StaticMaps staticMaps) => CrateMap.BitwiseAND(staticMaps.GoalMap).Equals(CrateMap);
        public bool IsSolutionReverse(StaticMaps staticMaps) => CrateMap.BitwiseAND(staticMaps.CrateStart).Equals(CrateMap);

        public class Comparer : IComparer<SolverNode>
        {
            public int Compare(SolverNode x, SolverNode y)
            {
                #if DEBUG
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                #endif

                if (x.hash > y.hash) return 1;            if (x.hash < y.hash) return -1;
                if (x.hashCrate > y.hashCrate) return 1;  if (x.hashCrate < y.hashCrate) return -1;
                if (x.hashMove > y.hashMove) return 1;    if (x.hashMove < y.hashMove) return -1;
                
                // Hashes the same, but may not be equal
                var cc = x.CrateMap.CompareTo(y.CrateMap);
                if (cc != 0) return cc;

                var cm = x.MoveMap.CompareTo(y.MoveMap);
                if (cm != 0) return cm;

                return 0;
            }
        }

       
    }
}