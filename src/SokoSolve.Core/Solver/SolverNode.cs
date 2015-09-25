using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Sokoban.Core.Analytics;
using Sokoban.Core.Common;
using Sokoban.Core.Primitives;

namespace Sokoban.Core.Solver
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
        public SolverNode()
        {
            Hash = int.MinValue;
        }

        public Bitmap CrateMap { get; set; }
        public Bitmap MoveMap { get; set; }
        public VectorInt2 PlayerBefore { get; set; }
        public VectorInt2 PlayerAfter { get; set; }
        public VectorInt2 CrateBefore { get; set; }
        public VectorInt2 CrateAfter { get; set; }

        public SolverNodeStatus Status { get; set; }

        public INodeEvaluator Evaluator { get; set; }

        public List<SolverNode> Duplicates { get; set; } 

        public int Goals { get; set; }

        private static int[] crateWeights = Primes.List;
        private static int[] moveWeights = Primes.List;

        private int hashAll = -1;
        private int hashCrate = -1;
        private int hashMove = -1;

        public int Hash { get; private set; }

        public void EnsureHash()
        {
            if (Hash == int.MinValue) GetHashCode();
        }

        public override int GetHashCode()
        {
            if (Hash != int.MinValue) return Hash;
            
            // Crate + Move can often optroximate ~= FloorMap
            hashCrate = CrateMap.GetHashCodeUsingPositionWeights(crateWeights);
            hashMove =  MoveMap.GetHashCodeUsingPositionWeights(moveWeights);
            hashAll = hashCrate | (hashMove << 4);

            return Hash = hashAll;
        }

        public bool Equals(IStateMaps other)
        {
            return CrateMap.Equals(other.CrateMap) && MoveMap.Equals(other.MoveMap);
        }

        public override bool Equals(object obj)
        {
            return Equals((IStateMaps)obj);
        }


        public int CompareTo(SolverNode other)
        {
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
            else
            {
                // Hashes the same, but not equal
                // Q: How do we convert != to <= and >=
                var cc = CrateMap.CompareTo(other.CrateMap);
                if (cc != 0) return cc;

                var cm = MoveMap.CompareTo(other.MoveMap);
                if (cm != 0) return cm;

                throw new InvalidOperationException();
            }
        }

        public override string ToString()
        {
            return string.Format("[#{0}] C{2} M{3} D{4} {5}", GetHashCode(), Goals, CrateMap.GetHashCode(), MoveMap.GetHashCode(), this.GetDepth(), Status);
        }

        public string ToStringDebug()
        {
            var map = new Map<char>(CrateMap.Size);
            map.Fill('.');
            foreach (var c in CrateMap.TruePositions())
            {
                map[c] = '#';
            }
            foreach (var m in MoveMap.TruePositions())
            {
                map[m] = 'p';
            }
            return map.ToString();
        }

        public string ToStringDebugPositions()
        {
            return string.Format("{0} PB{1}, PA{2}; CB{3}, CA{4}", ToString(), PlayerBefore, PlayerAfter, CrateBefore, CrateAfter);
        }

        public void AddDuplicate(SolverNode newKid)
        {
            if (Duplicates == null) Duplicates = new List<SolverNode>();
            Duplicates.Add(newKid);
        }

        public new SolverNode[] Children
        {
            get
            {
                if (!HasChildren) return new SolverNode[0];
                return base.Children.Cast<SolverNode>().ToArray();
            }
        }

        public new SolverNode Parent
        {
            get { return (SolverNode)base.Parent;  }
        }

        public void CheckDead()
        {
            if (Children.All(x => x.Status == SolverNodeStatus.Dead || x.Status == SolverNodeStatus.DeadRecursive))
            {
                Status = SolverNodeStatus.DeadRecursive;
                if (Parent != null)
                {
                    Parent.CheckDead();    
                }
            }
        }


    }
}