using System;
using System.Collections.Generic;
using System.Linq;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver.Components;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.NodeFactory;
using SokoSolve.Core.Solver.Queue;
using SokoSolve.Core.Solver.Solvers;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{


    public class SolverBuilder
    {
        public SolverBuilder(ISolverContainer globalContainer)
        {
            GlobalContainer = globalContainer;
            CompLibrary     = globalContainer.GetInstanceRequired<LibraryComponent>();
        }
        
        public LibraryComponent CompLibrary     { get; }
        public ISolverContainer GlobalContainer { get; }


        public static ISolverContainer BuildGlobalContainer(LibraryComponent compLib, ISokobanSolutionRepository? repSol)
        {
            var res = new SolverContainerByType();
            res.Register<LibraryComponent>(t=>compLib);
            if (repSol != null)
            {
                res.Register<ISokobanSolutionRepository>(t=>repSol);    
            }
            
            return res;
        }

        public static readonly IReadOnlyList<SimpleArgMeta> Arguments = new[]
        {
            new SimpleArgMeta("puzzle",    null,   "Puzzle",                      LargestRegularlySolvedPuzzleId),
            new SimpleArgMeta("solver",    "s",    "Solver",                      SolverFactoryDefault),
            new SimpleArgMeta("pool",      "p",    "Pool/Lookup",                 LookupFactoryDefault),
            new SimpleArgMeta("queue",     "q",    "Queue",                       QueueFactoryDefault),
            new SimpleArgMeta("min",       "m",    "Stop after x Minutes",        3),
            new SimpleArgMeta("sec",       "s",    "Stop after x Sec",            0),
            
            new SimpleArgMeta("cat",       null,   "Display Report in Console",   true),
            new SimpleArgMeta("safe",      null,   "Safe Mode",                   SafeMode.Off),
            new SimpleArgMeta("track",     null,   "Track Solutions",             false),
            new SimpleArgMeta("sol",       null,   "Compare against known solution"),
            
            // Exit Conditions
            new SimpleArgMeta("minR",      null,   "Min puzzle Rating Filter",    0),
            new SimpleArgMeta("maxR",      null,   "Max puzzle Rating Filter",    int.MaxValue),
            new SimpleArgMeta("maxNodes",  null,   "Max Total Nodes"),
            new SimpleArgMeta("maxDead",   null,   "Max Dead Nodes"),
            new SimpleArgMeta("stop",      null,   "Stop On Solution",            true),
            
            // Batch Args 
            new SimpleArgMeta("stopOnFails",  null,   "Stop After Consecutive Fails",           5),
            new SimpleArgMeta("skipSol",      null,   "Skip Puzzles with Solutions",            false),
        };

        private static readonly IReadOnlyDictionary<string, string> Defaults = SimpleArgs.FromMeta(Arguments);
        
        
        public const string DefaultPuzzle = "SQ1~P5";
        public const string LargestRegularlySolvedPuzzleId = "SQ1~P15";
        
        public        Action<SolverCommand>? GlobalEnrichCommand { get; set; }
        public        Action<SolverState>?   GlobalEnrichState   { get; set; }
        
        public const string QueueFactoryDefault = "qd";
         public static readonly NamedFactory<SolverCommand, ISolverQueue> QueueFactory = new NamedFactory<SolverCommand, ISolverQueue>()
                .Register("q"    , (x) => new SolverQueue())
                .Register("q!"   , (x) => new SolverQueueConcurrent())
                .Register("qd"   , (x) => new SolverQueueSortedWithDeDup())
            ;

        
        public const string LookupFactoryDefault = "bb:bst:lt";
        public static readonly NamedFactory<SolverCommand, INodeLookup> LookupFactory = new NamedFactory<SolverCommand, INodeLookup>()
                .Register("lock:bst:lt"   , (x) => new NodeLookupSlimRwLock(new NodeLookupBinarySearchTree(new NodeLookupLongTerm())))
                .Register("lock:bb:bst:lt", (x) => new NodeLookupSlimRwLock(new NodeLookupDoubleBuffered(new NodeLookupBinarySearchTree(new NodeLookupLongTerm()))))
                .Register("bb:ll:lt"      , (x) => new NodeLookupDoubleBuffered(new NodeLookupSortedLinkedList(new NodeLookupLongTerm())))
                .Register("bb:lock:ll:lt" , (x) => new NodeLookupDoubleBuffered(new NodeLookupSlimRwLock(new NodeLookupSortedLinkedList(new NodeLookupLongTerm()))))
                .Register("bb:lock:sl:lt" , (x) => new NodeLookupDoubleBuffered(new NodeLookupSlimRwLock(new NodeLookupSortedList(new NodeLookupLongTerm()))))
                .Register("bb:bst:lt"     , (x) => new NodeLookupDoubleBuffered(new NodeLookupBinarySearchTree(new NodeLookupLongTerm())))
                .Register("bb:lock:bst:lt", (x) => new NodeLookupDoubleBuffered(new NodeLookupSlimRwLock(new NodeLookupBinarySearchTree(new NodeLookupLongTerm()))))
                .Register("bb:lock:bucket", (x) => new NodeLookupDoubleBuffered( new NodeLookupSlimRwLock(new NodeLookupByBucket())))
                .Register("bb:bucket"     , (x) => new NodeLookupDoubleBuffered(new NodeLookupByBucket()))
                .Register("lock:bucket"   , (x) => new NodeLookupSlimRwLock(new NodeLookupByBucket()))
                .Register("bucket"        , (x) => new NodeLookupByBucket())
                .Register("baseline"      , (x) => new NodeLookupSlimRwLock(new NodeLookupSimpleList()))
            ;

        public const string SolverFactoryDefault = "fr!";
        public static readonly NamedFactory<SolverCommand, ISolver> SolverFactory = new NamedFactory<SolverCommand, ISolver>()
                .Register("f"     , (x) => new SingleThreadedForwardSolver(new SolverNodePoolingFactoryDefault()))
                .Register("r"     , (x) => new SingleThreadedReverseSolver(new SolverNodePoolingFactoryDefault()))
                .Register("fr"    , (x) => new SingleThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault()))
                .Register("fr!"   , (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryDefault()))
                .Register("fr!p"  , (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryPoolingConcurrentBag()))
                .Register("fr!py" , (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryPoolingConcurrentBag("index")))
                .Register("fr!pz" , (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryPoolingConcurrentBag("byteseq")))
                .Register("fr!P"  , (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryPooling()))
                .Register("f!pz"  , (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryPoolingConcurrentBag("byteseq"))
                {
                    ThreadCountReverse = 1,
                    ThreadCountForward = Environment.ProcessorCount
                })
                .Register("fr!pz11", (x) => new MultiThreadedForwardReverseSolver(new SolverNodePoolingFactoryPoolingConcurrentBag("byteseq"))
                {
                    ThreadCountReverse = 1,
                    ThreadCountForward = 1
                })
            ;
        

       
        
        public SolverState BuildFrom(PuzzleIdent ident, IReadOnlyDictionary<string, string> buildArgs,
            Action<SolverCommand>? enrichCommand = null, Action<SolverState>? enrichState = null)
            => BuildFrom(CompLibrary.GetPuzzleWithCaching(ident).Puzzle, ident, buildArgs, enrichCommand, enrichState);

        public SolverState BuildFrom(LibraryPuzzle puzzle, IReadOnlyDictionary<string, string> buildArgs,
            Action<SolverCommand>? enrichCommand = null, Action<SolverState>? enrichState = null)
            => BuildFrom(puzzle.Puzzle, puzzle.Ident, buildArgs, enrichCommand, enrichState);

        public SolverState BuildFrom(Puzzle puzzle, PuzzleIdent ident, IReadOnlyDictionary<string, string> buildArgs, 
            Action<SolverCommand>? enrichCommand = null, Action<SolverState>? enrichState = null)
        {
            var args = SimpleArgs.Create(Arguments, buildArgs);
            
            var cmd = BuildCommand(puzzle, ident, args);
            enrichCommand?.Invoke(cmd);
            GlobalEnrichCommand?.Invoke(cmd);

            var solver = SolverFactory.GetInstance(cmd, args["solver"]);
            var state  = solver.Init(cmd);
            enrichState?.Invoke(state);
            GlobalEnrichState?.Invoke(state);
            return state;
        }
    

        public SolverCommand BuildCommand(Puzzle puz, PuzzleIdent ident, SimpleArgs args)
        {
            var exits = BuildExit(args);
            var container = new SolverContainerByType();
            var cmd = new SolverCommand(puz, ident, exits, container);
            cmd.GeneralArgs = args;
            if (args.TryGetValue("safe", out var textSafe))
            {
                cmd.SafeMode = Enum.Parse<SafeMode>(textSafe);
            }
            
            InitContainer(container, args);
            InitContainer(container, cmd, args);
            return cmd;
        }
        
        public SolverContainerByType BuildContainer(SimpleArgs args)
        {
            var res = new SolverContainerByType();
            InitContainer(res, args);
            return res;
        }
        
        void InitContainer(SolverContainerByType container, SimpleArgs args)
        {
            var repSol = GlobalContainer.GetInstance<ISokobanSolutionRepository>();
            
            SokobanSolutionComponent? compSol = null;
            
            // Components pass into this class
            container.Register<LibraryComponent>( _ => CompLibrary);
            if (repSol != null)
            {
                container.Register<ISokobanSolutionRepository>( _ => repSol);
            }

            if (args["track"] == true.ToString())
            {
                if (repSol == null) throw new NotSupportedException();
                compSol = new SokobanSolutionComponent(repSol);
                container.Register<ISokobanSolutionComponent>( _ =>compSol);
            }
            if (args.TryGetValue("sol", out var sol))// Track Solution
            {
                if (repSol == null) throw new NotSupportedException();
                container.Register<IKnownSolutionTracker>( _ => new KnownSolutionTracker( repSol, int.Parse(sol)));
            }
            
            //container.Register<ISolverRunTracking>( _ => TODO);

            
        }

        void InitContainer(SolverContainerByType container, SolverCommand cmd, SimpleArgs args)
        {
            container.Register<INodeLookup>(  _ => LookupFactory.GetInstance(cmd, args["pool"]));
            container.Register<ISolverQueue>( _ => QueueFactory.GetInstance(cmd, args["queue"]));
        }

        private ExitConditions BuildExit(IReadOnlyDictionary<string, string> args)
        {
            var ret = new ExitConditions();

            args.TryGetValue("min", out var min);
            args.TryGetValue("sec", out var sec);

            min ??= "0";
            sec ??= "0";
            
            ret.Duration =   TimeSpan.FromMinutes(int.Parse(min)) + TimeSpan.FromSeconds(int.Parse(sec));
            
            
            if (args.TryGetValue("stop", out var stop) && bool.Parse(stop))
            {
                ret.StopOnSolution = true;
            }
            
            if (args.TryGetValue("maxNodes", out var maxNodes) && int.TryParse(maxNodes, out var imaxNodes))
            {
                ret.MaxNodes = imaxNodes;
            }
            
            if (args.TryGetValue("maxDead", out var maxDead) && int.TryParse(maxDead, out var imaxDead))
            {
                ret.MaxDead = imaxDead;
            }
            
            return ret;
        }
        
        public class NamedFactory<TConfig, T>
        {
            private readonly Dictionary<string, Func<TConfig, T>> items = new();

            public NamedFactory<TConfig, T> Register(string name,  Func<TConfig, T> create)
            {
                items[name] = create;
                return this;
            }

            public T GetInstance(TConfig config, string name)
            {
                if (items.TryGetValue(name, out var create))
                {
                    return create(config);
                }
                throw new Exception($"Not Found: {name}; avail = {FluentString.Join(items.Keys)}");
            }

            public IEnumerable<string> GetAllKeys() => items.Keys;
        }




     
        
    }
}