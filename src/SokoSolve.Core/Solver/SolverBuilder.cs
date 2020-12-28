using System;
using System.Collections.Generic;
using SokoSolve.Core.Lib;
using SokoSolve.Core.Lib.DB;
using SokoSolve.Core.Solver.Lookup;
using SokoSolve.Core.Solver.Lookup.Lookup;
using SokoSolve.Core.Solver.NodeFactory;
using SokoSolve.Core.Solver.Queue;
using SokoSolve.Core.Solver.Solvers;
using TextRenderZ;

namespace SokoSolve.Core.Solver
{
    public class SolverBuilder
    {
        private readonly LibraryComponent compLib;
        private readonly IReadOnlyDictionary<string, string> defaults = new Dictionary<string, string>()
        {
            {"solver", SolverFactoryDefault},
            {"pool", LookupFactoryDefault},
            {"queue", QueueFactoryDefault},
            {"min", "3"}
        };
        public const string DefaultPuzzle = "SQ1~P5";
        public const string LargestRegularlySolvedPuzzleId = "SQ1~P15";
        
        public        Action<SolverCommand>? GlobalEnrichCommand { get; set; }
        public        Action<SolverState>?   GlobalEnrichState   { get; set; }
        
        public const string QueueFactoryDefault = "qd";
         public static readonly NamedFactory<SolverCommand, ISolverQueue> QueueFactory = new NamedFactory<SolverCommand, ISolverQueue>()
                .Register("q"   , (x) => new SolverQueue())
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
        

        public SolverBuilder(LibraryComponent compLib)
        {
            this.compLib = compLib;
        }
        
        public SolverState BuildFrom(PuzzleIdent ident, IReadOnlyDictionary<string, string> buildArgs,
            Action<SolverCommand>? enrichCommand = null, Action<SolverState>? enrichState = null)
            => BuildFrom(compLib.GetPuzzleWithCaching(ident).Puzzle, ident, buildArgs, enrichCommand, enrichState);

        public SolverState BuildFrom(LibraryPuzzle puzzle, IReadOnlyDictionary<string, string> buildArgs,
            Action<SolverCommand>? enrichCommand = null, Action<SolverState>? enrichState = null)
            => BuildFrom(puzzle.Puzzle, puzzle.Ident, buildArgs, enrichCommand, enrichState);

        public SolverState BuildFrom(Puzzle puzzle, PuzzleIdent ident, IReadOnlyDictionary<string, string> buildArgs, 
            Action<SolverCommand>? enrichCommand = null, Action<SolverState>? enrichState = null)
        {
            var args = new Dictionary<string, string>(defaults);
            foreach (var pair in buildArgs)
            {
                args[pair.Key] = pair.Value;
            }
            var cmd = BuildCommand(puzzle, ident, args);
            enrichCommand?.Invoke(cmd);
            GlobalEnrichCommand?.Invoke(cmd);

            var solver = SolverFactory.GetInstance(cmd, args["solver"]);
            var state  = solver.Init(cmd);
            enrichState?.Invoke(state);
            GlobalEnrichState?.Invoke(state);
            return state;
        }
    

        public SolverCommand BuildCommand(Puzzle puz, PuzzleIdent ident, IReadOnlyDictionary<string, string> args)
        {
            var exits = BuildExit(args);
            var container = new SolverContainerByType();
            var cmd = new SolverCommand(puz, ident, exits, container);
            if (args.TryGetValue("safe", out var textSafe))
            {
                cmd.SafeMode = Enum.Parse<SafeMode>(textSafe);
            }
            InitContainer(container, cmd, args);
            return cmd;
        }

        void InitContainer(SolverContainerByType container, SolverCommand cmd, IReadOnlyDictionary<string, string> args)
        {
            container.Register<INodeLookup>(  _ => LookupFactory.GetInstance(cmd, args["pool"]));
            container.Register<ISolverQueue>( _ => QueueFactory.GetInstance(cmd, args["queue"]));
            container.Register<LibraryComponent>( _ => compLib);
            
            // TODO
            //container.Register<ISokobanSolutionComponent>( _ => TODO);
            //container.Register<ISolverRunTracking>( _ => TODO);

            if (args.ContainsKey("track"))
            {
                container.Register<ISokobanSolutionComponent>( _ => throw new Exception());
            }
        }

        private ExitConditions BuildExit(IReadOnlyDictionary<string, string> args)
        {
            var ret = new ExitConditions();
            if (args.TryGetValue("min", out var min))
            {
                ret.Duration = TimeSpan.FromMinutes(int.Parse(min));
            }
            // TODO: TotalNodes, StopOnSolution, ...
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