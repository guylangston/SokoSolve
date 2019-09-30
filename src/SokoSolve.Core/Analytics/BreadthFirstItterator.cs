using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Common;

namespace SokoSolve.Core.Analytics
{


    public interface INodeEvaluator<T>
    {
        void Evalulate(T node, IStragetgyItterator<T> itterator);
    }

    public interface IStragetgyItterator<T>
    {
        List<T> Evaluate(T startNode, INodeEvaluator<T> evaluator, ExitConditions exitConditions);

        void AddSolution(T solution);

        void Add(T nodeNode);
        void Add(IEnumerable<T> nodeNodes);

        void Remove(T node);
    }

    public class ExitConditions
    {
        public TimeSpan TimeLimit { get; set; }
        public int MaxNodes { get; set; }
        public bool StopOnFirstSolution { get; set; }
    }


    public class BreadthFirstItterator<T> : IStragetgyItterator<T> where T: class, ITreeNode
    {
        private List<T> current = new List<T>();
        private List<T> next = new List<T>();
        private List<T> solutions = new List<T>();


        public List<T> Evaluate(T startNode, INodeEvaluator<T> evaluator, ExitConditions exitConditions)
        {
            current.Add(startNode);
            while (true) 
            {
                while (current.Count > 0)
                {
                    var n = SelectNextForEvaluation(current);
                    if (n == null) return solutions;
                    evaluator.Evalulate(n, this);
                    current.Remove(n);
                }
                if (next.Count > 0)
                {
                    // Swap
                    var s = current;
                    current = next;
                    next = s;
                }
                else
                {
                    return solutions;
                }
            }
        }

        public void AddSolution(T solution)
        {
            solutions.Add(solution);
        }

        private T SelectNextForEvaluation(List<T> list)
        {
            return list.First();
        }

        public void Add(T nodeNode)
        {
            if (nodeNode.Parent == null) throw new Exception("Parent must be set");
            next.Add(nodeNode);
        }

        public void Add(IEnumerable<T> nodeNodes)
        {
            next.AddRange(nodeNodes);
        }

        public void Remove(T node)
        {
            if (current.Contains(node)) current.Remove(node);
            if (next.Contains(node)) next.Remove(node);
        }
    }
}