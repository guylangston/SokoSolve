Starting Solver Run... solve --puzzle SQ1~P7 --experimental
PID: 102871
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.18.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit db7b7531860def7ef687e0c46255c8d84d3700b6
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Feb 27 14:13:25 2026 +0000
GIT-LOG1:     perf: optimised conditionals in FloodFill
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: Changes not staged for commit:
GIT-STAT:       modified:   ConsoleSolver.cs
GIT-STAT:       modified:   Program.cs
GIT-STAT: no changes added to commit (use "git add" and/or "git commit -a")
MemTotal:       65705556 kB
MemFree:        43189804 kB
MemAvailable:   50808204 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=591,222,737. sizeof(NodeStructWord)=2
Available Puzzles: 1
----------------------------------------------
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
CMP SolverCoordinator:                       LS-v1.1--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                                BlockSize: 100000
CMP INodeBacklog:                            SokoSolve.LargeSearchSolver.NodeBacklog
CMP ILNodeLookup:                            SokoSolve.LargeSearchSolver.Lookup.LNodeLookupBlackRedTree
CMP LNodeStructEvaluatorForwardExperimental: EXPERIMENTAL: DropVectorInt2
CMP INodeHashCalculator:                     SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:15,520,000 Heap:15,522,823 Backlog: [G 100%]      2,823(-2838) 1%  Lookup(62.3mil count:15,522,824 col:38) 3.5min
Completed:   00:03:28.4184963
Memory used: 2617MB
Total nodes: 15,529,013 at 74,508.8nodes/sec
Result:      SOLUTION!(1)
Summary:
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine     | Version                           |
SQ1~P7 | 60     | 208.4     | 15529013 | 1         | guyzen-arch | LS-v1.1--ForwardOnly+SingleThread |
