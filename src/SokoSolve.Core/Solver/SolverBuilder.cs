using System;
using System.Collections.Generic;
using System.Linq;
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
    public class SimpleArgMeta
    {
        public SimpleArgMeta(string name, string? s, string description, string? def, bool required)
        {
            Name        = name;
            Short       = s;
            Description = description;
            Default     = def;
            Required    = required;
        }

        public string  Name        { get; }    // --Name
        public string? Short       { get; }   // -s
        public string  Description { get; }
        public string? Default     { get; }
        public bool    Required    { get; }
        
        public object? Tag { get; set; }
    }
    
    
    public class SolverBuilder
    {
        private readonly LibraryComponent compLib;
        

        public static readonly IReadOnlyList<SimpleArgMeta> Arguments = new[]
        {
            new SimpleArgMeta("puzzle",    null,   "Puzzle",                      LargestRegularlySolvedPuzzleId, false),
            new SimpleArgMeta("solver",    "s",    "Solver",                      SolverFactoryDefault, false),
            new SimpleArgMeta("pool",      "p",    "Pool/Lookup",                 LookupFactoryDefault, false),
            new SimpleArgMeta("queue",     "q",    "Queue",                       QueueFactoryDefault,  false),
            new SimpleArgMeta("min",       "m",    "Stop after x Minutes",        3.ToString(), false),
            new SimpleArgMeta("sec",       "s",    "Stop after x Sec",            0.ToString(), false),
            
            new SimpleArgMeta("cat",       null,   "Display Report in Console",   true.ToString(), false),
            new SimpleArgMeta("safe",      null,   "Safe Mode",                   SafeMode.Off.ToString(), false),
            new SimpleArgMeta("track",     null,   "Track Solutions",             false.ToString(), false),
            
            // Exit Conditions
            new SimpleArgMeta("minR",      null,   "Min puzzle Rating Filter",    0.ToString(), false),
            new SimpleArgMeta("maxR",      null,   "Max puzzle Rating Filter",    int.MaxValue.ToString(), false),
            new SimpleArgMeta("maxNodes",  null,   "Max Total Nodes",             null, false),
            new SimpleArgMeta("maxDead",   null,   "Max Dead Nodes",              null, false),
            new SimpleArgMeta("stop",      null,   "Stop On Solution",            true.ToString(), false),
            
            
        };
        
        public static readonly IReadOnlyDictionary<string, string> Defaults = 
            Arguments.Where(x=>x.Default != null)
                     .ToDictionary(x => x.Name, x => x.Default!);
        
        
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
            var args = new Dictionary<string, string>(Defaults);
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
            cmd.GeneralArgs = args;
            if (args.TryGetValue("safe", out var textSafe))
            {
                cmd.SafeMode = Enum.Parse<SafeMode>(textSafe);
            }
            InitContainer(container, cmd, args);
            return cmd;
        }

        void InitContainer(SolverContainerByType container, SolverCommand cmd, IReadOnlyDictionary<string, string> args)
        {
            container.Register<LibraryComponent>( _ => compLib);
            
            container.Register<INodeLookup>(  _ => LookupFactory.GetInstance(cmd, args["pool"]));
            container.Register<ISolverQueue>( _ => QueueFactory.GetInstance(cmd, args["queue"]));
            if (args["track"] == true.ToString())
            {
                container.Register<ISokobanSolutionComponent>( _ => throw new Exception());
            }
            
            
            // TODO
            //container.Register<ISokobanSolutionComponent>( _ => TODO);
            //container.Register<ISolverRunTracking>( _ => TODO);

            
        }

        private ExitConditions BuildExit(IReadOnlyDictionary<string, string> args)
        {
            var ret = new ExitConditions();
            if (args.TryGetValue("min", out var min))
            {
                ret.Duration = TimeSpan.FromMinutes(int.Parse(min));
            }
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




        public static string GenerateCommandLine(IReadOnlyDictionary<string, string> aa) => 
            FluentString.Join(aa.Where(x => {
                if (SolverBuilder.Defaults.TryGetValue(x.Key, out var y))
                {
                    return y != x.Value;
                }
                return true;
            }),
            new JoinOptions()
            {
                Sep       = " ",
                WrapAfter = 100
            }, (s, pair) => s.Append($"--{pair.Key} {pair.Value}"));
        
        public static  void SetFromCommandLine(Dictionary<string, string> aa, string[] args)
        {
            var cc = 0;
            while (cc < args.Length)
            {
                if (args[cc].StartsWith("--"))
                {
                    var name = args[cc].Remove(0, 2);
                    if (cc + 1 < args.Length && !args[cc+1].StartsWith("--"))
                    {
                        aa[name] = args[cc + 1];
                        cc++;
                    }
                    else
                    {
                        aa[name] = true.ToString();  // flag
                    }
                }
                else if (cc == 0)
                {
                    aa["puzzle"] = args[0]; // default 1st puzzle
                }
                cc++;
            }
        }
    }
}