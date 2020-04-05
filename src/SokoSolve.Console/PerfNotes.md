- Added BenchmarkDotNet -- the full solver / queues are too slow for microbenchmarks
- Added SolverNodeLookupThreadSafeBuffer (vs SolverNodeLookupThreadSafeWrapper
- Added SolverQueueConcurrent (vs ThreadSafeSolverQueueWrapper)

Very good perf improvementL: 
 ```
256'000 SQ1~P5 "Grim Town" ==> 4,228,922 nodes at 23,489.0 nodes/sec. COMPLETED. *NO* Solutions. Continue [Duration 3 min]
20'000 SQ1~P5 "Grim Town" ==> 6,174,678 nodes at 34,295.3 nodes/sec. COMPLETED. *NO* Solutions. Continue [Duration 3 min]
```

2020-04-05 -- Better SolverNodeLookup strategies, some profiling improvements:
```cmd
GUYZEN running RT:3.1.3 OS:'WIN 6.2.9200.0' Threads:32 RELEASE x64 'AMD Ryzen Threadripper 2950X 16-Core Processor '
Git: '2786d45 Added System.CommandLine' at 2020-04-05 10:23:54Z, v3.1.0
[SQ1~P5] NoSolution.  24,198,600 nodes at 134,415/s in 3 min.

guyBuntu running RT:3.1.3 OS:'Unix 5.3.0.45' Threads:12 RELEASE x64 'AMD Ryzen 5 3600X 6-Core Processor'
Git: '2786d45 Added System.CommandLine' at 2020-04-05 10:29:47Z, v3.1.0
[SQ1~P5] NoSolution.  18,196,361 nodes at 101,078/s in 3 min.

WILLOW running RT:3.1.3 OS:'WIN 6.2.9200.0' Threads:12 RELEASE x64 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz'
Git: '2786d45 Added System.CommandLine' at 2020-04-05 10:26:48Z, v3.1.0
[SQ1~P5] NoSolution.  11,932,402 nodes at 66,263/s in 3 min.
```