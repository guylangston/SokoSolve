using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using SokoSolve.Core.Analytics;
using SokoSolve.Core.Common;
using SokoSolve.Core.Primitives;
using VectorInt;

namespace SokoSolve.Core.Solver
{
    public enum SolverNodeStatus
    {
        // Workflow
        None,
        UnEval,
        InProgress,
        
        // Single Node Result
        Evaluated,      // has children (who have not be evaluated)
        Duplicate,      
        Solution,
        Dead,           // no children
        
        // Child Recursive
        DeadRecursive,  // all kids are dead
        SolutionPath,   
    }

    public struct NodeStatusCounts // TODO: Microbenchmark if vs array[(int)enum]++
    {
        int None;
        int UnEval;
        int InProgress;
        int Evaluated;      
        int Duplicate;      
        int Solution;
        int Dead; 
        int DeadRecursive;  
        int SolutionPath;   

        public int this[SolverNodeStatus s]
        {
            get
            {
                var ss = (int)s;
                if (ss == 0) return None;
                if (ss == 1) return UnEval++;
                if (ss == 2) return InProgress;
                if (ss == 3) return Evaluated;
                if (ss == 4) return Duplicate;
                if (ss == 5) return Solution;
                if (ss == 6) return Dead;
                if (ss == 7) return DeadRecursive;
                if (ss == 8) return SolutionPath;

                throw new InvalidDataException();
            }
        }

        public void Inc(SolverNodeStatus s)
        {
            var ss = (int)s;
            if (ss == 0) None++;
            else if (ss == 1) UnEval++;
            else if (ss == 2) InProgress++;
            else if (ss == 3) Evaluated++;
            else if (ss == 4) Duplicate++;
            else if (ss == 5) Solution++;
            else if (ss == 6) Dead++;
            else if (ss == 7) DeadRecursive++;
            else if (ss == 8) SolutionPath++;
        }
        
        public void Dec(SolverNodeStatus s)
        {
            var ss = (int)s;
            if (ss == 0) None--;
            else if (ss == 1) UnEval--;
            else if (ss == 2) InProgress--;
            else if (ss == 3) Evaluated--;
            else if (ss == 4) Duplicate--;
            else if (ss == 5) Solution--;
            else if (ss == 6) Dead--;
            else if (ss == 7) DeadRecursive--;
            else if (ss == 8) SolutionPath--;
        }

        public int Open   => None + UnEval + InProgress + Evaluated;
        public int Closed => Duplicate + Solution + Dead + SolutionPath + DeadRecursive;
        public int Total  => Open + Closed;

    }

    public enum PushDirection
    {
        Up, Down,
        Left, Right
    }

   

    public class SolverNodeRoot : SolverNode
    {
        public SolverNodeRoot(
            VectorInt2 playerBefore, VectorInt2 push, 
            IBitmap crateMap, IBitmap moveMap, INodeEvaluator evaluator, Puzzle puzzle) 
            : base(null, playerBefore, push, crateMap, moveMap)
        {
            Evaluator = evaluator;
            Puzzle = puzzle;
        }

        public override  INodeEvaluator Evaluator { get; }
        public Puzzle Puzzle { get;  }
    }

    public interface ISolverNodeDuplicateLink
    {
        SolverNode Duplicate { get; set; }
    }

    /// <summary>
    /// Centralise all tree logic/methods. Allows experimenting with tree data structure: children as LinkList,Array,etc
    /// </summary>
    public abstract class SolverNodeTreeFeatures : ITreeNode 
    {
        private SolverNode? firstChild;
        private SolverNode? nextSibling;
        
        // Up Methods
        public SolverNode Parent { get; protected set; }
        public SolverNode              Root()       => TreeNodeHelper.Root((SolverNode)this);
        public IEnumerable<SolverNode> PathToRoot() => TreeNodeHelper.PathToRoot((SolverNode)this);
        public int                     GetDepth()   => TreeNodeHelper.GetDepth(this);
        
        // Down Methods
        public bool HasChildren => firstChild != null;
        public IEnumerable<SolverNode> Children 
        {
            get
            {
                if (firstChild == null) yield break;
                yield return firstChild;
                var next = firstChild.nextSibling;
                while (next != null)
                {
                    yield return next;
                    next = next.nextSibling;
                }
            }
        }

        public void SetChildren(IEnumerable<SolverNode> kids)
        {
            lock (this)
            {
                foreach (var kid in kids)
                {
                    AddInner(kid);
                }
            }
        }

        public void Add(SolverNode kid)
        {
            lock(this)
            {
                AddInner(kid);
            }
        }
        
        private void AddInner(SolverNode kid)
        {

            ((SolverNodeTreeFeatures)kid).Parent = (SolverNode)this;
            if (firstChild == null)
            {
                firstChild = kid;
            }
            else
            {
                var next = firstChild;
                while (next.nextSibling != null)
                {
                    next = next.nextSibling;
                }

                next.nextSibling = kid;
            }
        }

        public IEnumerable<SolverNode> Recurse() => TreeNodeHelper.RecursiveAll((SolverNode)this);
        public IEnumerable<SolverNode> RecurseOpen() => TreeNodeHelper.Where((SolverNode)this, x=>x.IsOpen);
        public int CountRecursive() => TreeNodeHelper.Count(this);

        IEnumerable<ITreeNode> ITreeNode.Children => Children;
        ITreeNodeParent ITreeNodeParent. Parent   => Parent;
        IEnumerator IEnumerable.GetEnumerator() => ((ITreeNode) this).GetEnumerator();
        IEnumerator<ITreeNode> IEnumerable<ITreeNode>.GetEnumerator() => Recurse().GetEnumerator();

        protected void InitialiseInstance(SolverNode parent)
        {
            Parent = parent;
            firstChild = null;
            nextSibling = null;
        }
    }

    public class SolverNodeDTO
    {
        public int             Hash     { get; set; }
        public IReadOnlyBitmap CrateMap { get; set; }
        public IReadOnlyBitmap MoveMap  { get; set; }
        public Puzzle Puzzle { get; set; }

        public override string ToString() => $"{Hash}<=>{CrateMap.GetHashCode()}:{MoveMap.GetHashCode()}";
    }
   
    
    public class SolverNode : SolverNodeTreeFeatures, IStateMaps, IEquatable<IStateMaps>, IComparable<SolverNode>
    {
        private static volatile int nextId = 1;
        
        private int hash;
        private int solverNodeId;
        private VectorByte2 playerBefore;
        private byte push;
        private byte status;
        private IBitmap crateMap;
        private IBitmap moveMap;

        public SolverNode(SolverNode? parent, VectorInt2 playerBefore, VectorInt2 push, IBitmap crateMap, IBitmap moveMap)
        {
#if DEBUG
            if (moveMap == null) throw new ArgumentNullException(nameof(moveMap));
            if (crateMap == null) throw new ArgumentNullException(nameof(crateMap));
#endif
            InitialiseInstance(parent, playerBefore, push, crateMap, moveMap, true);
        }
        
        public SolverNode(SolverNode? parent, VectorInt2 playerBefore, VectorInt2 push, IBitmap crateMap, IBitmap moveMap, int id)
        {
#if DEBUG
            if (moveMap == null) throw new ArgumentNullException(nameof(moveMap));
            if (crateMap == null) throw new ArgumentNullException(nameof(crateMap));
#endif
            solverNodeId = id;
            InitialiseInstance(parent, playerBefore, push, crateMap, moveMap, false);
        }
        
        public void InitialiseInstance(SolverNode parent, VectorInt2 playerBefore, VectorInt2 push, IBitmap crateMap, IBitmap moveMap, bool setId)
        {
            base.InitialiseInstance(parent);

            if (setId)
            {
                // Check init/use should have a NEW id to avoid same-ref bugs; it is effectively a new instance
                solverNodeId = Interlocked.Increment(ref nextId);    
            }

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

            hash = CalcHashCode(crateMap, moveMap);
        }

        public static int CalcHashCode(IBitmap crate, IBitmap move)
        {
            unchecked
            {
                var hashCrate = crate.GetHashCode();
                var hashMove  = move.GetHashCode();
                return hashMove ^ (hashCrate << 16);
                
                // #if NET47
                // hash =  hashCrate ^ (hashMove << (MoveMap.Width / 2));
                // #else
                // hash = HashCode.Combine(hashCrate, hashMove);    // Seems to give different runtime answers
                // #endif
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

        public virtual INodeEvaluator Evaluator =>
            this.Root() is SolverNodeRoot sr
                ? sr.Evaluator
                : throw new InvalidCastException($"Root node must be of type: {nameof(SolverNodeRoot)}, but got {this.Root().GetType().Name}");

        public new SolverNode? Parent => (SolverNode) base.Parent;
  

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
            => $"Id:{SolverNodeId}->{Parent?.SolverNodeId} #{GetHashCode()}/C{CrateMap.GetHashCode()}/M{MoveMap.GetHashCode()} D{this.GetDepth()} Kids:{Children?.Count()} {Status}";


        public NodeStatusCounts GetStatusCountsRecursive(bool inclusive = true)
        {
            var r = new NodeStatusCounts();     // stack alloc?
            if (inclusive) r.Inc(this.Status);
            if (HasChildren)
            {
                foreach (var n in Children)
                {
                    r.Inc(n.Status);
                }
            }
            return r;
        }

        public int CheckDead()
        {
            if (HasChildren)
            {
                var counts = GetStatusCountsRecursive(false);    // includes this node which should be evaluated
                if (counts[SolverNodeStatus.Solution] > 0)
                {
                    Status = SolverNodeStatus.SolutionPath;
                }
                if (counts.Open == 0)
                {
                    Status = SolverNodeStatus.DeadRecursive;
                    return (Parent?.CheckDead() ?? 0) + 1;
                }
            }
            return 0;
        }
        
        public bool IsClosed => Status == SolverNodeStatus.Dead || Status == SolverNodeStatus.DeadRecursive ||
                                Status == SolverNodeStatus.Solution || Status == SolverNodeStatus.SolutionPath;

        public bool IsOpen => !IsClosed;
        
        // TODO: Could be optimised? AND and COMPARE seems expensive
        public bool IsSolutionForward(StaticMaps staticMaps) => CrateMap.BitwiseAND(staticMaps.GoalMap).Equals(CrateMap);
        public bool IsSolutionReverse(StaticMaps staticMaps) => CrateMap.BitwiseAND(staticMaps.CrateStart).Equals(CrateMap);

        public class ComparerFull : IComparer<SolverNode>
        {
            public int Compare(SolverNode x, SolverNode y)
            {
                #if DEBUG
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (x.MoveMap == null) throw new ArgumentNullException(nameof(x.MoveMap));
                if (x.CrateMap == null) throw new ArgumentNullException(nameof(x.CrateMap));
                if (y == null) throw new ArgumentNullException(nameof(y));
                if (y.MoveMap == null) throw new ArgumentNullException(nameof(y.MoveMap));
                if (y.CrateMap == null) throw new ArgumentNullException(nameof(y.CrateMap));
                #endif

                if (x.hash > y.hash) return 1;            
                if (x.hash < y.hash) return -1;
                
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
                if (x == null) throw new ArgumentNullException(nameof(x));
                if (y == null) throw new ArgumentNullException(nameof(y));
#endif

                if (x.hash > y.hash) return 1;            
                if (x.hash < y.hash) return -1;
                
                return 0;
            }
        }

        public string ToStringMaps()
        {
            var map = new Map<char>(CrateMap.Size);
            map.Fill('.');
            foreach (var p in CrateMap.TruePositions())
            {
                map[p] = 'C';
            }
            foreach (var p in MoveMap.TruePositions())
            {
                map[p] = 'M';
            }
            return map.ToString();
        }

        
    }
}

