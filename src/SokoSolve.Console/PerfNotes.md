- Added BenchmarkDotNet -- the full solver / queues are too slow for microbenchmarks
- Added SolverNodeLookupThreadSafeBuffer (vs SolverNodeLookupThreadSafeWrapper
- Added SolverQueueConcurrent (vs ThreadSafeSolverQueueWrapper)

Very good perf improvementL: 1m -> 6m on "Grim Town"