using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SokoSolve.Core.Solver
{
    public interface ISolver 
    {
        SolverStatistics[]  Statistics         { get; }
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
    
    public interface IProgressNotifier
    {
        void Update(ISolver caller, SolverCommandResult state, SolverStatistics global);
    }
    
    public interface ISolverQueue : ISolverVisualisation
    {
        SolverStatistics Statistics { get; }

        void Enqueue(SolverNode              node);
        void Enqueue(IEnumerable<SolverNode> nodes);

        SolverNode?   Dequeue();
        SolverNode[]? Dequeue(int count);
    }

    public interface ISolverNodeLookup : ISolverVisualisation
    {
        SolverStatistics Statistics { get; }

        void Add(SolverNode              node);
        void Add(IReadOnlyCollection<SolverNode> nodes);

        SolverNode? FindMatch(SolverNode find);
    }
}