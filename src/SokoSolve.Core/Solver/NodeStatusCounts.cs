using System.Collections.Generic;
using System.IO;
using TextRenderZ;

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

    public enum PushDirection
    {
        Up, Down,
        Left, Right
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

        public override string ToString() =>
            FluentString.Create()
                .If(None > 0,$"N:{None}").Sep(",")
                .If(UnEval > 0,$"U:{UnEval}").Sep(",")
                .If(InProgress > 0,$"P:{InProgress}").Sep(",")
                .If(Evaluated > 0,$"E:{Evaluated}").Sep(",")
                .If(Duplicate > 0,$"D:{Duplicate}").Sep(",")
                .If(Solution > 0,$"S:{Solution}").Sep(",")
                .If(Dead > 0,$"D:{Dead}").Sep(",")
                .If(DeadRecursive > 0,$"DR:{DeadRecursive}").Sep(",")
                .If(SolutionPath > 0,$"SP:{SolutionPath}")
                .Append("/")
                .Append(Total.ToString())
                .ToString();

        public static NodeStatusCounts Count(IEnumerable<SolverNode> nodes)
        {
            var res = new NodeStatusCounts();
            foreach (var node in nodes)
            {
                res.Inc(node.Status);
            }

            return res;
        }
    }
}
