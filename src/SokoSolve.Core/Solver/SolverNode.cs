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

    public enum PushDirection
    {
        Up, Down,
        Left, Right
    }

    public class FatSolverNode : SolverNode
    {
        public int               Goals      { get; }
        public List<SolverNode>? Duplicates { get; set; }

        public FatSolverNode(VectorInt2 playerBefore, VectorInt2 push,
            IBitmap crateMap, IBitmap moveMap, int goals, List<SolverNode>? duplicates) : base(playerBefore, push, crateMap, moveMap)
        {
            Goals = goals;
            Duplicates = duplicates;
        }

        public void AddDuplicate(SolverNode newKid)
        {
            if (Duplicates == null) Duplicates = new List<SolverNode>();
            Duplicates.Add(newKid);
        }
    }

    public class SolverNodeRoot : SolverNode
    {
        public SolverNodeRoot(
            VectorInt2 playerBefore, VectorInt2 push, 
            IBitmap crateMap, IBitmap moveMap, INodeEvaluator evaluator, Puzzle puzzle) 
            : base(playerBefore, push, crateMap, moveMap)
        {
            Evaluator = evaluator;
            Puzzle = puzzle;
        }

        public INodeEvaluator Evaluator { get; }
        public Puzzle Puzzle { get;  }
    }

   
    
    public class SolverNode : TreeNodeBaseFixedKids, IStateMaps, IEquatable<IStateMaps>, IComparable<SolverNode>
    {
        private static volatile int nextId = 1;
        
        private int hash;
        private int solverNodeId;
        private VectorByte2 playerBefore;
        private byte push;
        private byte status;
        private IBitmap crateMap;
        private IBitmap moveMap;

        public SolverNode(VectorInt2 playerBefore, VectorInt2 push, IBitmap crateMap, IBitmap moveMap)
        {
            InitialiseInstance(playerBefore, push, crateMap, moveMap);
        }
        
        public void InitialiseInstance(VectorInt2 playerBefore, VectorInt2 push, IBitmap crateMap, IBitmap moveMap)
        {
            base.Clear();
            
            // Check init/use should have a NEW id to avoid same-ref bugs; it is effectively a new instance
            solverNodeId = Interlocked.Increment(ref nextId);
            
            this.playerBefore = new VectorByte2(playerBefore);
            this.push         = push switch
            {
                (0,  0) => (byte)0,
                (0, -1) => (byte)1,
                (0,  1) => (byte)2,
                (-1, 0) => (byte)3,
                (1,  0) => (byte)4,
                _ => throw new ArgumentOutOfRangeException(push.ToString())
            };
            this.crateMap     = crateMap;
            this.moveMap      = moveMap;
            this.status       = (byte)SolverNodeStatus.UnEval;

            unchecked
            {
                var hashCrate = CrateMap.GetHashCode();
                var hashMove  = MoveMap.GetHashCode();
                #if NET47
                hash =  hashCrate ^ (hashMove << (MoveMap.Width / 2));
                #else
                hash = HashCode.Combine(hashCrate, hashMove);
                #endif
            }
        }

        public int              SolverNodeId => solverNodeId;
        public VectorInt2       PlayerBefore => new VectorInt2(playerBefore.X, playerBefore.Y);
        public IBitmap          CrateMap     => crateMap;
        public IBitmap          MoveMap      => moveMap;
        public VectorInt2       PlayerAfter  => PlayerBefore + Push;
        public VectorInt2       CrateBefore  => PlayerAfter + Push;
        public VectorInt2       CrateAfter   => CrateBefore + Push;
        public SolverNodeStatus Status
        {
            get => (SolverNodeStatus) status;
            set => status = (byte)value;
        }

        public VectorInt2 Push => push switch
        {
            0 => new VectorInt2(0, 0),
            1 => new VectorInt2(0, -1),
            2 => new VectorInt2(0, 1),
            3 => new VectorInt2(-1, 0),
            4 => new VectorInt2(1, 0),
            _ => throw new ArgumentOutOfRangeException(push.ToString())
        };

        public INodeEvaluator Evaluator =>
            this.Root() is SolverNodeRoot sr
                ? sr.Evaluator
                : throw new InvalidCastException($"Root node must be of type: {nameof(SolverNodeRoot)}, but got {this.Root().GetType().Name}");

        public new SolverNode? Parent => (SolverNode) base.Parent;
        public new IEnumerable<SolverNode>? Children => HasChildren 
            ? base.Children.Cast<SolverNode>() 
            : ImmutableArray<SolverNode>.Empty;

        public static readonly IComparer<SolverNode> ComparerInstanceFull = new ComparerFull();
        public static readonly IComparer<SolverNode> ComparerInstanceHashOnly = new ComparerHashOnly();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(SolverNode other) => ComparerInstanceFull.Compare(this, other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(IStateMaps other) 
            => other != null && (CrateMap.Equals(other.CrateMap) && MoveMap.Equals(other.MoveMap));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => hash;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => Equals((IStateMaps) obj);

        public override string ToString()
            => $"[Id:{SolverNodeId} #{GetHashCode()}] C{CrateMap.GetHashCode()} M{MoveMap.GetHashCode()} D{this.GetDepth()} {Status}";
        

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

        public class ComparerFull : IComparer<SolverNode>
        {
            public int Compare(SolverNode x, SolverNode y)
            {
                #if DEBUG
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                #endif

                if (x.hash > y.hash) return 1;            if (x.hash < y.hash) return -1;
                
                // Hashes the same, but may not be equal
                var cc = x.CrateMap.CompareTo(y.CrateMap);
                if (cc != 0) return cc;

                var cm = x.MoveMap.CompareTo(y.MoveMap);
                if (cm != 0) return cm;

                return 0;
            }
        }
        
        public class ComparerHashOnly : IComparer<SolverNode>
        {
            public int Compare(SolverNode x, SolverNode y)
            {
                #if DEBUG
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                #endif

                if (x.hash > y.hash) return 1;            if (x.hash < y.hash) return -1;
                
                return 0;
            }
        }

       
    }
}
