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
        SolverCommandResult Init(SolverCommand command);
        
        void Solve(SolverCommandResult state);
    }

    public interface ISolverVisualisation
    {
        bool TrySample(out SolverNode? node);        // false = not supported
    }

    /// <summary>
    /// Prove more information that can be inferred from GetType()
    /// </summary>
    public interface IExtendedFunctionalityDescriptor
    {
        string TypeDescriptor { get; }
        IEnumerable<(string name, string text)> GetTypeDescriptorProps(SolverCommandResult state);  // throws NoSupported
    }
    
    public interface IProgressNotifier
    {
        void Update(ISolver caller, SolverCommandResult state, SolverStatistics global);
    }
    
    public interface ISolverQueue : ISolverVisualisation, IExtendedFunctionalityDescriptor
    {
        SolverStatistics Statistics { get; }

        void Enqueue(SolverNode              node);
        void Enqueue(IEnumerable<SolverNode> nodes);

        SolverNode?   Dequeue();
        SolverNode[]? Dequeue(int count);
    }

    public interface ISolverNodeLookup : ISolverVisualisation, IExtendedFunctionalityDescriptor
    {
        SolverStatistics Statistics { get; }

        void Add(SolverNode              node);
        void Add(IReadOnlyCollection<SolverNode> nodes);

        SolverNode? FindMatch(SolverNode find);
        
        // For debugging; may not be threadsafe
        IEnumerable<SolverNode> GetAll();
    }
}