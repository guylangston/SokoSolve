===[Solver Header]===    solve --puzzle SQ1~P7 --experimental
PID: 106210
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.18.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit 8158e30a80ad97a406b870fa333ea7b7460b82b4
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Feb 27 14:35:13 2026 +0000
GIT-LOG1:     feat: compact report
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65705556 kB
MemFree:        42223820 kB
MemAvailable:   49839224 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=579,946,542. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]===    Bob's Cottage
Puzzle:              Bob's Cottage
Ident:               SQ1~P7
Rating:              60
Size:                (13,10)
Known-Size-Solution: 15,529,013
~~###########
~##.....#..P#
###.X.XX#...#
#.##X....XX.#
#..#..X.#...#
######.######
#OO.OOX.#$##~
#.OO....###~~
#..OO#####~~~
#########~~~~
Flags: EXPERIMENTAL
CML NodeStruct:                              v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator:                       LS-v1.1--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                                BlockSize: 100000
CMP INodeBacklog:                            SokoSolve.LargeSearchSolver.NodeBacklog
CMP ILNodeLookup:                            SokoSolve.LargeSearchSolver.Lookup.LNodeLookupBlackRedTree
CMP LNodeStructEvaluatorForwardExperimental: EXPERIMENTAL: DropVectorInt2
CMP INodeHashCalculator:                     SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:15,520,000 Heap:15,522,823 Backlog: [G 100%]      2,823(-2838) 1%  Lookup(62.3mil count:15,522,824 col:33) 3.4min
Completed:   00:03:24.8922092
Memory used: 2616MB
Total nodes: 15,529,013 at 75,791.1nodes/sec
Result:      SOLUTION!(1)
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine     | Version                           |
SQ1~P7 | 60     | 204.9     | 15529013 | 1         | guyzen-arch | LS-v1.1--ForwardOnly+SingleThread |



===[Solver Header]===    solve --puzzle SQ1~P7 --non-interactive
PID: 115351
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.18.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit 3910d2771dc3364bc514860e532108103356da9f
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Feb 27 15:00:48 2026 +0000
GIT-LOG1:     fix: friendly name; feat: NodeStruct as a component
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: Changes not staged for commit:
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/NodeBacklog.cs
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/PROGRESS-BENCH-guyzen.md
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/SolverCoordinatorFactory.cs
GIT-STAT: no changes added to commit (use "git add" and/or "git commit -a")
MemTotal:       65705556 kB
MemFree:        49036148 kB
MemAvailable:   56742932 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=660,287,022. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]===    Bob's Cottage
Puzzle:              Bob's Cottage
Ident:               SQ1~P7
Rating:              60
Size:                (13,10)
Known-Size-Solution: 15,529,013
~~###########
~##.....#..P#
###.X.XX#...#
#.##X....XX.#
#..#..X.#...#
######.######
#OO.OOX.#$##~
#.OO....###~~
#..OO#####~~~
#########~~~~
CML NodeStruct:                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator:                 LS-v1.1--ForwardOnly+SingleThread
CMP NodeHeap:                          BlockSize: 100000
CMP NodeBacklog:                       BlockSize: 100000
CMP ILNodeLookup:                      SokoSolve.LargeSearchSolver.Lookup.LNodeLookupBlackRedTree
CMP LNodeStructEvaluatorForwardStable: DropVectorInt2
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
Completed:   00:03:14.0368238
Memory used: 2616MB
Total nodes: 15,529,013 at 80,031.3nodes/sec
Result:      SOLUTION!(1)
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine     | Version                           |
SQ1~P7 | 60     | 194.0     | 15529013 | 1         | guyzen-arch | LS-v1.1--ForwardOnly+SingleThread |

===[Solver Header]===    solve --puzzle SQ1~P7
PID: 21311
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.19.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit ca4b0a4a264195658ed5775731efae533152d65e
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Tue Mar 24 07:17:11 2026 +0000
GIT-LOG1:     refactor: removed legacy ref from testing
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: Changes not staged for commit:
GIT-STAT:       modified:   watch-mem.sh
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/SolverCoordinator.cs
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/interfaces_Coordinator.cs
GIT-STAT:       modified:   ../SokoSolve.Primitives/TestLibrary.cs
GIT-STAT: no changes added to commit (use "git add" and/or "git commit -a")
MemTotal:       65704848 kB
MemFree:        52396472 kB
MemAvailable:   55867912 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=76. TheorticalNodeLimit=752,746,603. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]=== puzzle attempt 1/1
Puzzle:              Bob's Cottage
Ident:               SQ1~P7
Rating:              60
Size:                (13,10)
KnownSolution:       323 steps
Contraints:          StopOnSolution
Known-Size-Solution: 15,529,013
~~###########
~##.....#..P#
###.X.XX#...#
#.##X....XX.#
#..#..X.#...#
######.######
#OO.OOX.#$##~
#.OO....###~~
#..OO#####~~~
#########~~~~
CMP NodeStruct:                                                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator->SolverCoordinator:                              LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking
CMP INodeHeap->NodeHeap:                                               BlockSize: 100000
CMP INodeBacklog->NodeBacklog:                                         BlockSize: 100000
CMP ILNodeLookup->LNodeLookupBlackRedTree:
CMP ILNodeStructEvaluator->LNodeStructEvaluatorForwardDeadChecks(fwd): v1.4:Dead
CMP ILNodeStructEvaluator->LNodeStructEvaluatorReverse(rev):           v0.1
CMP INodeHashCalculator:                                               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
CMP INodeWatcher->SolutionTracker:
CMP ISolverCoordinatorDebugger:                                        (null)
CMP ISolverCoodinatorPeek:                                             SokoSolve.LargeSearchSolver.Console.SolverCoodinatorPeekConsole
CMP ISolverCoordinatorFactory->SolverCoordinatorFactory:

SolutionTracker: TrackSol=47+46/103(90%)
Completed:       00:01:08.4437750
Memory used:     812MB
Total nodes:     4,737,089 at 69,211.4nodes/sec
Dead:            714,608
Result:          SOLUTION!
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes   | Solutions | Machine     | Version                                            |
SQ1~P7 | 60     | 68.4      | 4737089 | 1         | guyzen-arch | LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking |
