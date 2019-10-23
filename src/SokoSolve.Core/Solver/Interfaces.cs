using System.Collections.Generic;

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
    
    public interface IProgressNotifier
    {
        void Update(ISolver caller, SolverCommandResult state, SolverStatistics global);
    }
    
    public interface ISolverQueue
    {
        SolverStatistics Statistics { get; }

        void Enqueue(SolverNode              node);
        void Enqueue(IEnumerable<SolverNode> nodes);

        SolverNode   Dequeue();
        SolverNode[] Dequeue(int count);
    }

    public interface ISolverNodeLookup
    {
        SolverStatistics Statistics { get; }

        void Add(SolverNode              node);
        void Add(IEnumerable<SolverNode> nodes);

        SolverNode FindMatch(SolverNode node);
    }
}