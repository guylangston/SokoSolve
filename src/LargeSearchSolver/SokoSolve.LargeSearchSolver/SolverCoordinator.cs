using SokoSolve.LargeSearchSolver.Utils;
using SokoSolve.Primitives;
using SokoSolve.Primitives.Analytics;

namespace SokoSolve.LargeSearchSolver;

public class SolverCoordinator : ISolverCoordinator, ISolverCoordinatorCallback, ISolverComponent
{
    public ISolverCoordinatorFactory StateFactory { get; init; } = new SolverCoordinatorFactory();
    public ISolverCoodinatorPeek? Peek { get; init; }

    public string GetComponentName()   => nameof(SolverCoordinator);
    public string Describe()           => SolverVersion;
    public static string SolverVersion => "LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking"; // T1=single threaded

    public void AssertSolution(LSolverState state, uint solutionNodeId)
    {
        if (state.Request.AttemptConstraints.StopOnSolution)
        {
            state.StopRequested = true;
        }
    }

    public void AssertSolution(LSolverState state, uint chainForwardId, uint chainReverseId)
    {
        if (state.Request.AttemptConstraints.StopOnSolution)
        {
            state.StopRequested = true;
        }
    }

    public LSolverState Init(LSolverRequest request)
    {
        var mapsStatic = new StaticAnalysisMaps(request.Puzzle);

        var nsContext = new NSContext(request.Puzzle.Width, request.Puzzle.Height, mapsStatic.FloorMap);
        if (StateFactory is SolverCoordinatorFactory scf)
        {
            scf.SetNSContext(nsContext);
            scf.InitComplete();
        }
        INodeWatcher? watcher = null;
        if (request.TrackSolution != null)
        {
            watcher = StateFactory.GetInstance<INodeWatcher>(request);
        }
        var state = new LSolverState
        {
            Request             = request,
            CoordinatorCallback = this,
            Coordinator         = this,
            StaticMaps          = new StaticAnalysisMaps(request.Puzzle),
            NodeStructContext   = nsContext,
            Heap                = StateFactory.GetInstance<INodeHeap>(request) ?? throw new NullReferenceException("Heap"),
            Lookup              = StateFactory.GetInstance<ILNodeLookup>(request) ?? throw new NullReferenceException("Lookup"),
            Backlog             = StateFactory.GetInstance<INodeBacklog>(request) ?? throw new NullReferenceException("Backlog"),
            EvalForward         = StateFactory.GetInstance<ILNodeStructEvaluator>(request, "Forward"),
            EvalReverse         = StateFactory.GetInstance<ILNodeStructEvaluator>(request, "Reverse"),
            HashCalculator      = StateFactory.GetInstance<INodeHashCalculator>(request) ?? throw new NullReferenceException("Hash"),
            NodeWatcher         = watcher,
            MemAvailAtStart     = OSHelper.GetMemoryFree(),
        };
        watcher?.Init(state);
        return state;
    }

    public IEnumerable<(string Name, string Desc)> DescribeComponents(LSolverState state)
    {
        yield return DescribeObj(this);
        yield return DescribeObj(state.Heap);
        yield return DescribeObj(state.Backlog);
        yield return DescribeObj(state.Lookup);
        yield return DescribeObj(state.EvalForward, "(fwd)");
        yield return DescribeObj(state.EvalReverse, "(rev)");
        yield return DescribeObj(state.HashCalculator);
        yield return DescribeObj(state.NodeWatcher);
        yield return DescribeObj(state.Debugger);
        yield return DescribeObj(Peek);
        yield return DescribeObj(StateFactory);

        (string Name, string Desc) DescribeObj<T>(T obj, string qual = "")
        {
            if (obj is ISolverComponent cmp)
            {
                return (typeof(T).Name + "->" + cmp.GetComponentName() +qual, cmp.Describe());
            }
            return (typeof(T).Name, obj?.ToString() ?? "(null)");
        }
    }

    public LSolverResult Solve(LSolverState state)
    {
        SolverStart(state);
        var cc = 0;
        var tickAt = Peek?.PeekEvery ?? 10_000;
        while(!state.StopRequested && state.Backlog.TryPop(out var nextNodeId))
        {
            state.Result.StatusTotalNodesEvaluated = cc;
            ref var node = ref state.Heap.GetById(nextNodeId);
#if DEBUG
            state.Debugger?.EvalStart(state, ref node);
#endif
            if (node.Type == NodeStruct.NodeType_Forward)
            {
                state.EvalForward?.Evaluate(state, ref node);
            }
            else // NodeType_Reverse
            {
                state.EvalReverse?.Evaluate(state, ref node);
            }
#if DEBUG
            state.Debugger?.EvalComplete(state, ref node);
#endif

            if (tickAt == 1 || cc % tickAt == 0)
            {
                if (!HandleTickCheck(state, cc, node))
                {
                    break;
                }
            }
            cc++;
        }

        SolverEnd(state);

        return state.Result;
    }

    private bool HandleTickCheck(LSolverState state, int cc, NodeStruct node)
    {
        if (state.Request.AttemptConstraints.MaxTime != null)
        {
            var elapsed = DateTime.Now - state.Started;
            if (elapsed.TotalSeconds > state.Request.AttemptConstraints.MaxTime.Value)
            {
                state.StopRequested = true;
                return false;
            }
        }
        if (state.Request.AttemptConstraints.MaxNodes is int maxNodes)
        {
            if (state.Result.StatusTotalNodesEvaluated > maxNodes)
            {
                state.StopRequested = true;
                return false;
            }
        }
        if (state.Request.AttemptConstraints.StopOnSwap)
        {
            if (OSHelper.UsingSwapMemory())
            {
                state.StopRequested = true;
                return false;
            }
        }

        if (Peek?.TickUpdate(state, cc, ref node) == false)
        {
            state.StopRequested = true;
            return false;
        }

        return true;
    }

    protected void SolverStart(LSolverState state)
    {
        state.Started = DateTime.Now;

        // Init the root nodes (fwd, rev)
        if (state.EvalForward != null)
        {
            state.RootForward = state.EvalForward.InitRoot(state);
            ref var node = ref state.Heap.GetById(state.RootForward);
            state.Backlog.Push( [state.RootForward] );
            state.Heap.Commit(ref node);
            state.Lookup.Add(ref node);
            state.NodeWatcher?.OnCommit(ref node);
        }
        if (state.EvalReverse != null)
        {
            state.RootReverse = state.EvalReverse.InitRoot(state);
            ref var node = ref state.Heap.GetById(state.RootReverse);
            state.Backlog.Push( [state.RootReverse] );
            state.Heap.Commit(ref node);
            state.Lookup.Add(ref node);
            state.NodeWatcher?.OnCommit(ref node);
        }

        if (state.EvalForward != null && state.EvalReverse != null)
        {
            if (!state.Request.AttemptConstraints.StopOnSolution)
            {
                throw new NotSupportedException("Does it make sense to keep looking when we are creating dups?");
            }
        }
    }

    protected void SolverEnd(LSolverState state)
    {
        state.Result.Exit = state.StopRequested
            ? "StopRequested"
            : state.Backlog.Count == 0  ? "Exhaustive" : "Unknown";

        state.Ended = DateTime.Now;
    }

    public Task<LSolverResult> SolveAsync(LSolverState state, CancellationToken cancel)
    {
        throw new NotImplementedException();
    }
}
