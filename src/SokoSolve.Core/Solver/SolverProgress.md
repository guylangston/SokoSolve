# Solver Progress/Performance Log


 ## 11-Apr-2014	 On WILLOW:

 ```

SingleThreadedForwardSolver_Trivial						147 nodes at 4081.68931957961 nodes/sec. COMPLETED. 1 solutions.
SingleThreadedForwardSolver_BaseLine					2227 nodes at 301.251696062605 nodes/sec. COMPLETED. 1 solutions.
SingleThreadedForwardSolver_TreePoolPerformance			6525 nodes at 109.888468526248 nodes/sec. COMPLETED. 1 solutions.
SingleThreadedForwardSolver_TreePoolPerformance_Large	3000 nodes at 24.518860996516 nodes/sec. Exited EARLY. Time => Nodes: 3000, Dead: 0, Duration: 00:02:02.3547864, Duplicates: 28457

** Added SolveNodeLookup (Bucketed List based on HashCode)

SingleThreadedForwardSolver_TreePoolPerformance_Large	3000 nodes at 28.4470246662833 nodes/sec. Exited EARLY. Time => Nodes: 3000, Dead: 0, Duration: 00:01:45.4591837, Duplicates: 27624
	?? This should have been much faster

** Debugged and Extended SolverNodeLookup

SingleThreadedForwardSolver_Trivial						157 nodes at 13568.9901041442 nodes/sec. COMPLETED. 1 solutions.
SingleThreadedForwardSolver_BaseLine					3755 nodes at 624.636997324208 nodes/sec. COMPLETED. 1 solutions.
SingleThreadedForwardSolver_TreePoolPerformance			10000 nodes at 155.93126115272 nodes/sec. Exited EARLY. Time => Nodes: 10000, Dead: 0, Duration: 00:01:04.1308223, Duplicates: 72684
SingleThreadedForwardSolver_TreePoolPerformance_Large	3000 nodes at 28.9283826347143 nodes/sec. Exited EARLY. Time => Nodes: 3000, Dead: 0, Duration: 00:01:43.7043805, Duplicates: 28084

** Optimised SolveNodeLookup, Bitmap.Equals

SingleThreadedForwardSolver_BaseLine					7878 nodes at 1877.12910473666 nodes/sec. COMPLETED. 1 solutions.
SingleThreadedForwardSolver_TreePoolPerformance_Large	8000 nodes at 126.824187591997 nodes/sec. Exited EARLY. Time => Nodes: 8000, Dead: 0, Duration: 00:01:03.0794500, Duplicates: 77198
	
** Optimised TreePool -- uses Ordered bands by Hash

SingleThreadedForwardSolver_TreePoolPerformance_Large	9000 nodes at 114.472844170638 nodes/sec. Exited EARLY. Time => Nodes: 9000, Dead: 0, Duration: 00:01:18.6212666, Duplicates: 0
SingleThreadedForwardSolver_Trivial						9193 nodes at 2926.01020000854 nodes/sec. COMPLETED. 1 solutions.

NOTE: It seems by changing the order from FIFO to an ordered list (where similar nodes are grouped together) dramatically increases the number of nodes 
		that need to be scaned. When you think about it for a moment, it makes sense.

** TreePool now uses SolverNodeList

SingleThreadedForwardSolver_TreePoolPerformance_Large	10000 nodes at 1009.81857710524 nodes/sec. Exited EARLY. TotalNodes => Nodes: 10000, Dead: 0, Duration: 00:00:09.9027689, Duplicates: 0
														31000 nodes at 496.305148257355 nodes/sec. Exited EARLY. Time => Nodes: 31000, Dead: 0, Duration: 00:01:02.4615725, Duplicates: 0
	Comment: Wow this is 10 times faster

```

## 13-Apr		On		BOLTHOLE

```
SingleThreadedForwardSolver_TreePoolPerformance			34000 nodes at 559.334934180624 nodes/sec. Exited EARLY. Time => Nodes: 34000, Dead: 0, Duration: 00:01:00.7864768, Duplicates: 0
```
