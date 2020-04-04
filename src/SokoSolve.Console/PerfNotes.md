- Added BenchmarkDotNet -- the full solver / queues are too slow for microbenchmarks
- Added SolverNodeLookupThreadSafeBuffer (vs SolverNodeLookupThreadSafeWrapper
- Added SolverQueueConcurrent (vs ThreadSafeSolverQueueWrapper)

Very good perf improvementL: 
 ```
256'000 SQ1~P5 "Grim Town" ==> 4,228,922 nodes at 23,489.0 nodes/sec. COMPLETED. *NO* Solutions. Continue [Duration 3 min]
20'000 SQ1~P5 "Grim Town" ==> 6,174,678 nodes at 34,295.3 nodes/sec. COMPLETED. *NO* Solutions. Continue [Duration 3 min]
```