using System;
using System.Collections.Generic;

namespace SokoSolve.Core.Solver
{
    public class SingleThreadedForwardSolver : SolverBase
    {
        public SingleThreadedForwardSolver(ISolverNodeFactory nodeFactory) 
            : base(new ForwardEvaluator(nodeFactory))
        {
        }

        public string GetTypeDescriptor => GetType().Name;
        public IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverState state) => throw new NotSupportedException();
    }
}