using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SokoSolve.Core.Solver
{
    public interface ISolver : IExtendedFunctionalityDescriptor
    {
        SolverStatistics[]?  Statistics         { get; }
        int                 VersionMajor       { get; }
        int                 VersionMinor       { get; }
        int                 VersionUniversal   { get; }
        string              VersionDescription { get; }
        SolverResult Init(SolverCommand command);
        
        void Solve(SolverResult state);
    }

    public interface ISolverVisualisation
    {
        bool TrySample(out SolverNode? node);        // false = not supported
        IEnumerable<SolverNode> GetAll();        // throw notsupported
    }

    /// <summary>
    /// Prove more information that can be inferred from GetType()
    /// </summary>
    public interface IExtendedFunctionalityDescriptor
    {
        string TypeDescriptor { get; }
        IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverResult state);  // throws NoSupported
    }
    
    public interface IProgressNotifier
    {
        void Update(ISolver caller, SolverResult state, SolverStatistics global, string txt);
    }

    public interface IStatisticsProvider
    {
        SolverStatistics Statistics { get; }    // Not real-time accurate, do not use for functionality, only reporting
    }
    
    public interface ISolverQueue : IStatisticsProvider, IExtendedFunctionalityDescriptor
    {
        void Enqueue(SolverNode node);
        void Enqueue(IEnumerable<SolverNode> nodes);

        SolverNode?   Dequeue();
        SolverNode[]? Dequeue(int count);
    }

    public interface ISolverPool : IStatisticsProvider, IExtendedFunctionalityDescriptor
    {
        void Add(SolverNode node);
        void Add(IReadOnlyCollection<SolverNode> nodes);

        SolverNode? FindMatch(SolverNode find);
    }

    public interface ISolverPoolChained : ISolverPool
    {
        ISolverPool InnerPool { get;  }
    }
    
    
    public interface ISolverPoolBatching : ISolverPool
    {
        int MinBlockSize { get; }
        bool IsReadyToAdd(IReadOnlyCollection<SolverNode> buffer);
    }
    
   
}