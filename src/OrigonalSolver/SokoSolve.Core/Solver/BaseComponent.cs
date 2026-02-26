using System.Collections.Generic;
using System.Collections.Immutable;

namespace SokoSolve.Core.Solver
{
    public abstract class BaseComponent : IExtendedFunctionalityDescriptor, IStatisticsProvider
    {
        protected BaseComponent()
        {
            TypeDescriptor = GetType().Name;
            Statistics = new SolverStatistics()
            {
                Name = TypeDescriptor,
                Type = TypeDescriptor,
            };
        }

        public string TypeDescriptor { get; protected set; }

        public virtual IEnumerable<(string name, string? text)> GetTypeDescriptorProps(SolverState state) => ImmutableArray<(string name, string? text)>.Empty;
        public SolverStatistics Statistics { get; }
    }
}