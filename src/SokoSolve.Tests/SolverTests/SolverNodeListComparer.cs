using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Solver;

namespace SokoSolve.Tests.SolverTests
{
    public class SolverNodeListComparer
    {
        public List<SolverNode> NotInA { get; } = new List<SolverNode>();
        
        public List<SolverNode>               NotInB { get; } = new List<SolverNode>();
        public List<(SolverNode, SolverNode)> DupA   { get; } = new List<(SolverNode, SolverNode)>();
        public List<(SolverNode, SolverNode)> DupB   { get; } = new List<(SolverNode, SolverNode)>();

        public int Compare(IEnumerable<SolverNode> aa, IEnumerable<SolverNode> bb)
        {
            var aaa = aa.OrderBy(x => x.GetHashCode()).ToArray();
            var bbb = bb.OrderBy(x => x.GetHashCode()).ToArray();
            
            var full = new SolverNode.ComparerFull();
            for (int i = 0; i < aaa.Length; i++)
            {
                if (i > 0 && full.Compare(aaa[i], aaa[i - 1]) == 0)
                {
                    DupA.Add((aaa[i], aaa[i-1]));
                }
                else
                {
                    var b = Array.BinarySearch(bbb, aaa[i], full);
                    if (b < 0)
                    {
                        NotInB.Add(aaa[i]);
                    }    
                }
            }
            
            for (int i = 0; i < bbb.Length; i++)
            {
                if (i > 0 && full.Compare(bbb[i], bbb[i - 1]) == 0)
                {
                    DupA.Add((bbb[i], bbb[i-1]));
                }
                else
                {
                    var a = Array.BinarySearch(aaa, bbb[i], full);
                    if (a < 0)
                    {
                        NotInA.Add(bbb[i]);
                    }    
                }
            }

            return NotInA.Count + NotInB.Count + DupA.Count + DupB.Count;
        }
    }
}