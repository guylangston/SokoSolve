using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

            unchecked
            {
                hashCrate = CrateMap.HashUsingWeights(crateWeights);
                hashMove  = MoveMap.HashUsingWeights(moveWeights);
                hash      = hashCrate ^ (hashMove << (MoveMap.Width / 2));
            }
        }

        public VectorInt2 PlayerBefore { get; }
        public VectorInt2 PlayerAfter  { get; }
        public VectorInt2 CrateBefore  { get; }
        public VectorInt2 CrateAfter   { get; }
        public Bitmap     CrateMap     { get; }
        public Bitmap     MoveMap      { get; }
        public INodeEvaluator? Evaluator { get;  }
        public int Goals { get;  }
        
        public SolverNodeStatus Status       { get; set; }
        public List<SolverNode>? Duplicates   { get; set; }

        public new SolverNode[] Children
        {
            get
            {
                if (!HasChildren) return new SolverNode[0];
                return base.Children.Cast<SolverNode>().ToArray();
            }
        }

        public new SolverNode Parent => (SolverNode) base.Parent;

        public int CompareTo(SolverNode other)
        {
            if (other == null) return 1;
            
            var all = GetHashCode().CompareTo(other.GetHashCode());
            if (all != 0) return all;

            var c = hashCrate.CompareTo(other.hashCrate);
            if (c != 0) return c;

            var m = hashMove.CompareTo(other.hashMove);
            if (m != 0) return m;

            if (Equals(other))
            {
                return 0;
            }

            // Hashes the same, but not equal
            // Q: How do we convert != to <= and >=
            var cc = CrateMap.CompareTo(other.CrateMap);
            if (cc != 0) return cc;

            var cm = MoveMap.CompareTo(other.MoveMap);
            if (cm != 0) return cm;

            throw new InvalidOperationException();
        }

        public bool Equals(IStateMaps other) => other == null 
            ? false 
            : CrateMap.Equals(other.CrateMap) && MoveMap.Equals(other.MoveMap);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => hash;

        public override bool Equals(object obj) => Equals((IStateMaps) obj);

        public override string ToString() 
            => $"[#{GetHashCode()}] C{CrateMap.GetHashCode()} M{MoveMap.GetHashCode()} D{this.GetDepth()} {Status}";
        public string ToStringDebugPositions() 
            => $"{ToString()} PB{PlayerBefore}, PA{PlayerAfter}; CB{CrateBefore}, CA{CrateAfter}";

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
            if (Children.All(x => x.Status == SolverNodeStatus.Dead || x.Status == SolverNodeStatus.DeadRecursive))
            {
                Status = SolverNodeStatus.DeadRecursive;
                if (Parent != null) Parent.CheckDead();
            }
        }
    }
}