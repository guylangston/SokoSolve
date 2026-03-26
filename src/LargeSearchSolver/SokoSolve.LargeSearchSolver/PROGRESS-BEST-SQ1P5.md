===[Solver Header]===    --pid solve --puzzle SQ1~P5 --veryLarge --stop-on-swap --tags Dead
PID: 15421 > ./sokosolve.pid
willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.106 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit ddf9b3e00f61750b597d4f8de7b4d6d4caa961d8
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Wed Mar 25 11:56:39 2026 +0000
GIT-LOG1:     feat: show bits needed; fix: unneeded alloc
MemTotal:       65782660 kB
MemFree:        63396396 kB
MemAvailable:   63862260 kB
SwapCached:         6768 kB
SwapTotal:       8388604 kB
SwapFree:        8352764 kB
sizeof(NodeStruct)=35. TheorticalNodeLimit=1,868,405,145
Available Puzzles: 1
===[Body]=== puzzle attempt 1/1
Puzzle:         Grim Town
Ident:          SQ1~P5
Rating:         76
Size:           (16,14)
KnownSolution:  505 steps
Bit/Map needed: 68/224=30% masked positions, 2x=18 bytes
Contraints:     StopOnSolution,StopOnSwap
Best-Attempt:   755,091,605
~~~~~~~~~~~#####
~~~~~~~~~~##...#
~~~~~~~~~~#....#
~~~~####~~#.X.##
~~~~#..####X.X#~
~~~~#.....X.X.#~
~~~##.##.X.X.X#~
~~~#..O#..X.X.#~
~~~#..O#......#~
#####.#########~
#OOOO.P..#~~~~~~
#OOOO....#~~~~~~
##..######~~~~~~
~####~~~~~~~~~~~
CMP NodeStruct:                                                        v2.1:CustomFloodFill,BitPacking,NSContext,BitmapMasked
CMP SolverCoordinator->SolverCoordinator:                              LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking
CMP INodeHeap->NodeHeap:                                               BlockSize: 10000000
CMP INodeBacklog->NodeBacklog:                                         BlockSize: 1000000
CMP ILNodeLookup->LNodeLookupCompound:                                 Sharing[16] -> Dynamic=LNodeLookupBlackRedTree(312500), Immutable[LNodeLookupImmutable]
CMP ILNodeStructEvaluator->LNodeStructEvaluatorForwardDeadChecks(fwd): v1.4:Dead
CMP ILNodeStructEvaluator->LNodeStructEvaluatorReverse(rev):           v0.1
CMP INodeHashCalculator:                                               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
CMP INodeWatcher->SolutionTracker:
CMP ISolverCoordinatorDebugger:                                        (null)
CMP ISolverCoodinatorPeek:                                             SokoSolve.LargeSearchSolver.Console.SolverCoodinatorPeekConsole
CMP ISolverCoordinatorFactory->SolverCoordinatorFactory:               Dead,MEMORY,VERYLARGE

SolutionTracker: TrackSol=52+63/195(58%)
Completed:       18:48:04.3954642
Memory used:     63291MB
Total nodes:     1,283,330,000 at 18,960.5nodes/sec
Dead:            1,390,954,161
Result:          FAILED
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes      | Solutions | Machine | Version                                            |
SQ1~P5 | 76     | 67684.4   | 1283330000 | 0         | willow  | LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking |
