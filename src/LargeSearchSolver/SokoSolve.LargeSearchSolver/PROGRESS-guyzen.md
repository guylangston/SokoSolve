# main dev machine
```
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.18.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit f37b2002067e7c4ed541112cc2ccb16f55948604
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Thu Feb 26 14:03:16 2026 +0000
GIT-LOG1:     feat: Linux memory
GIT-STAT: On branch SokoSolve.Primitives
GIT-STAT: Your branch is ahead of 'origin/SokoSolve.Primitives' by 1 commit.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65705552 kB
MemFree:        51229116 kB
MemAvailable:   58660996 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=682,600,680. sizeof(NodeStructWord)=2
```

> `dotnet run -c Release -- solve --puzzle __known --maxTime 3600`

```
Puzzle  | Rating | Time(sec) | Nodes    | Solutions | Machine     | Version                           |
SQ1~P3  | 20     | 0.0       | 946      | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P1  | 19     | 0.1       | 3047     | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P27 | 30     | 0.1       | 3368     | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P17 | 26     | 0.4       | 64253    | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P21 | 33     | 1.0       | 137931   | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P15 | 49     | 2.3       | 260656   | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P39 | 41     | 7.2       | 788481   | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P29 | 40     | 8.4       | 913786   | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P41 | 45     | 30.0      | 3077343  | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P7  | 60     | 219.8     | 15529013 | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P43 | 66     | 319.8     | 16701691 | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
```

---

```
$ dotnet run -c Release -- solve --puzzle SQ1  --maxTime 60
SQ1~P1  | 19     | 0.1       | 3047    | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P3  | 20     | 0.0       | 946     | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P17 | 26     | 0.6       | 64253   | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P27 | 30     | 0.0       | 3368    | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P21 | 33     | 1.0       | 137931  | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P29 | 40     | 8.6       | 913786  | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P39 | 41     | 7.2       | 788481  | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P41 | 45     | 30.6      | 3077343 | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P15 | 49     | 2.3       | 260656  | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P7  | 60     | 60.1      | 4900000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P43 | 66     | 60.0      | 3750000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P25 | 74     | 60.1      | 3100000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P5  | 76     | 60.1      | 2850000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P11 | 83     | 60.1      | 2680000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P47 | 102    | 60.1      | 1930000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P35 | 163    | 60.2      | 950000  | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P9  | 290    | 60.6      | 760000  | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
```

```
Puzzle  | Rating | Time(sec) | Nodes     | Solutions | Machine     | Version                           |
SQ1~P25 | 74     | 3600.1    | 142530000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P5  | 76     | 3600.1    | 103470000 | 0         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
```

> Optimised MyBitmapSpan.SetBitwiseOR (20sec faster)
```
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine     | Version                           |
SQ1~P7 | 60     | 207.9     | 15529013 | 1         | guyzen-arch | LS-v1.0--ForwardOnly+SingleThread |
```

-----------------[Largest Attempt: Failed]-----------------------------------------------------------------

===[Solver Header]===    solve --puzzle SQ1~P5 --veryLarge
PID: 135452
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.18.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit e10024232f46fa4db9620c1332382ca2631d93a8
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Feb 27 16:25:44 2026 +0000
GIT-LOG1:     misc
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65705556 kB
MemFree:        55564824 kB
MemAvailable:   62366260 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=725,716,480. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]===    Grim Town
Puzzle: Grim Town
Ident:  SQ1~P5
Rating: 76
Size:   (16,14)
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
Flags: VERYLARGE
CML NodeStruct:                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator:                 LS-v1.1--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                          BlockSize: 1000000
CMP NodeBacklog:                       BlockSize: 100000
CMP LNodeLookupCompound:               Dynamic=LNodeLookupBlackRedTree(1000000), Immutable[LNodeLookupImmutable]
CMP LNodeStructEvaluatorForwardStable: DropVectorInt2
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:233,060,000 Heap:267,323,745 Backlog:34,263,745( 3207) 100%  1.0days
Completed:   1.01:08:37.0302744
Memory used: 24802MB
Total nodes: 233,060,000 at 2,574.8nodes/sec
Result:      FAILED
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes     | Solutions | Machine     | Version                           |
SQ1~P5 | 76     | 90517.0   | 233060000 | 0         | guyzen-arch | LS-v1.1--ForwardOnly+SingleThread |


-------[Trying Sharding]---------------------------------

===[Solver Header]===    solve --puzzle SQ1~P7 --veryLarge
PID: 166494
guyzen-arch 'AMD Ryzen Threadripper 2950X 16-Core Processor' OS:Unix6.18.9.1 dotnet:10.0.3 Threads:32 x64 RELEASE
GIT-LOG1: commit 5790cca91659d16a74c8984689bcff3e297ba365
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Sat Feb 28 10:04:16 2026 +0000
GIT-LOG1:     add: scratch(histogram); add: verions to NodeEvaluator
GIT-STAT: On branch master
GIT-STAT: Your branch is ahead of 'origin/master' by 1 commit.
GIT-STAT: Changes not staged for commit:
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/Lookup/LNodeLookupImmutable.cs
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/PROGRESS-guyzen.md
GIT-STAT:       modified:   ../SokoSolve.LargeSearchSolver/SolverCoordinatorFactory.cs
GIT-STAT: Untracked files:
GIT-STAT:       ../SokoSolve.LargeSearchSolver/Lookup/LNodeLookupSharding.cs
GIT-STAT: no changes added to commit (use "git add" and/or "git commit -a")
MemTotal:       65705556 kB
MemFree:        49499524 kB
MemAvailable:   57600108 kB
SwapCached:            0 kB
SwapTotal:       4194300 kB
SwapFree:        4194300 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=670,252,869. sizeof(NodeStructWord)=2
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
Flags: VERYLARGE
CML NodeStruct:                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator:                 LS-v1.1--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                          BlockSize: 1000000
CMP NodeBacklog:                       BlockSize: 100000
CMP LNodeLookupCompound:               Sharing[16] -> Dynamic=LNodeLookupBlackRedTree(312500), Immutable[LNodeLookupImmutable]
CMP LNodeStructEvaluatorForwardStable: v1.2:DropVectorInt2
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:15,530,000 Heap:15,530,463 Backlog: [G 100%]        463(-3037) 0%  3.0min
Completed:   00:02:59.6723562
Memory used: 1929MB
Total nodes: 15,531,082 at 86,441.1nodes/sec
Result:      SOLUTION!(1)
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine     | Version                           |
SQ1~P7 | 60     | 179.7     | 15531082 | 1         | guyzen-arch | LS-v1.1--ForwardOnly+SingleThread |
