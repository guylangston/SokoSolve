willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.85 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit f42712fbdbed62d9a8b13fb0f279fa72beb01f45
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Thu Feb 26 15:12:39 2026 +0000
GIT-LOG1:     progress notes
GIT-STAT: On branch SokoSolve.Primitives
GIT-STAT: Your branch is up to date with 'origin/SokoSolve.Primitives'.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65782712 kB
MemFree:        59641464 kB
MemAvailable:   63929844 kB
SwapCached:            0 kB
SwapTotal:       8388604 kB
SwapFree:        8388604 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=743,910,912. sizeof(NodeStructWord)=2

Puzzle  | Rating | Time(sec) | Nodes     | Solutions | Machine | Version                           |
SQ1~P25 | 74     | 3600.3    | 138830000 | 0         | willow  | LS-v1.0--ForwardOnly+SingleThread |
SQ1~P5  | 76     | 3600.0    | 98390000  | 0         | willow  | LS-v1.0--ForwardOnly+SingleThread |

## Before MyBitmap.SetBitwiseOR
```
willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.101 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit f42712fbdbed62d9a8b13fb0f279fa72beb01f45
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Thu Feb 26 15:12:39 2026 +0000
GIT-LOG1:     progress notes
GIT-STAT: On branch SokoSolve.Primitives
GIT-STAT: Your branch is up to date with 'origin/SokoSolve.Primitives'.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65782608 kB
MemFree:        64323240 kB
MemAvailable:   64381260 kB
SwapCached:            0 kB
SwapTotal:       8388604 kB
SwapFree:        8388604 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=749,163,752. sizeof(NodeStructWord)=2

Available Puzzles: 1
----------------------------------------------
Puzzle: Bob's Cottage
Ident:  SQ1~P7
Rating: 60
Size:   (13,10)
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
CMP SolverCoordinator:                 LS-v1.0--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                          BlockSize: 100000
CMP INodeBacklog:                      SokoSolve.LargeSearchSolver.NodeBacklog
CMP ILNodeLookup:                      SokoSolve.LargeSearchSolver.Lookup.LNodeLookupBlackRedTree
CMP LNodeStructEvaluatorForwardStable: STABLE
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:15,520,000 Heap:15,522,823 Backlog:     2,823(-2838) 0%  Lookup(62.3mil count:15,522,824 col:31) 231.2sec
Completed:   00:03:51.2558505
Memory used: 2619MB
Total nodes: 15,529,013 at 67,150.8nodes/sec
Result:      SOLUTION!(1)
Summary:
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine | Version                           |
SQ1~P7 | 60     | 231.3     | 15529013 | 1         | willow  | LS-v1.0--ForwardOnly+SingleThread |
```
## After
```
```



------------- [ Very long attempt --- stalled and stopped]---------------------
Starting Solver Run... --puzzle SQ1~P5
PID: 3246
willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.101 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit 1b5dcc1ce4bb650eb6a5a705e9b7ee767eeaceda
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Feb 27 11:57:38 2026 +0000
GIT-LOG1:     feat: --veryLarge
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65782608 kB
MemFree:        63825612 kB
MemAvailable:   63953592 kB
SwapCached:            0 kB
SwapTotal:       8388604 kB
SwapFree:        8388604 kB
sizeof(NodeStruct)=88. TheorticalNodeLimit=744,187,252. sizeof(NodeStructWord)=2

Available Puzzles: 1
----------------------------------------------
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
CMP SolverCoordinator:                 LS-v1.1--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                          BlockSize: 1000000
CMP INodeBacklog:                      SokoSolve.LargeSearchSolver.NodeBacklog
CMP LNodeLookupCompound:               Dynamic=LNodeLookupBlackRedTree, Immutable[LNodeLookupImmutable]
CMP LNodeStructEvaluatorForwardStable: STABLE
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:289,080,000 Heap:328,023,732 Backlog:38,943,732(  562) 13%  123,321.0sec
ompleted:   1.10:15:20.9912361
Memory used: 30457MB
Total nodes: 289,080,000 at 2,344.1nodes/sec
Result:      FAILED
Summary:
Puzzle | Rating | Time(sec) | Nodes     | Solutions | Machine | Version                           |
SQ1~P5 | 76     | 123321.0  | 289080000 | 0         | willow  | LS-v1.1--ForwardOnly+SingleThread |


------------[Largest Attempt, crash:memory? ]-------------------------------
===[Solver Header]===    --pid solve --puzzle SQ1~P5 --veryLarge
PID: 8787 > ./sokosolve.pid
willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.101 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit 919df00b198f3dc302bfc1ccb052df1a6ddf3c00
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Sat Feb 28 20:51:40 2026 +0000
GIT-LOG1:     rules: --veryLarge used LNodeLookupCompoundResize rather the origonal
GIT-LOG1:     Compound
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: nothing to commit, working tree clean
MemTotal:       65782608 kB
MemFree:        63489348 kB
MemAvailable:   64056000 kB
SwapCached:            0 kB
SwapTotal:       8388604 kB
SwapFree:        8388604 kB
sizeof(NodeStruct)=84. TheorticalNodeLimit=780,873,142. sizeof(NodeStructWord)=2
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
CMP NodeHeap:                          BlockSize: 10000000
CMP NodeBacklog:                       BlockSize: 1000000
CMP LNodeLookupCompound:               Sharing[16] -> Dynamic=LNodeLookupBlackRedTree(312500), Immutable[LNodeLookupImmutable]
CMP LNodeStructEvaluatorForwardStable: v1.2:DropVectorInt2
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:681,440,000 Heap:755,091,605 Backlog:73,651,605(-1040) 100%  5.4hr

--------------------[2nd largest]------------------------------
===[Solver Header]===    solve --puzzle SQ1~P25 --tags Dead
PID: 17620
willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.101 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit 0543fe5c11bb9d00c846ea9220fe157e7edee667
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Thu Mar 12 21:53:51 2026 +0000
GIT-LOG1:     feat: more useful report for solution nodes only
MemTotal:       65782612 kB
MemFree:        61952516 kB
MemAvailable:   64102520 kB
SwapCached:            0 kB
SwapTotal:       8388604 kB
SwapFree:        8388604 kB
sizeof(NodeStruct)=84. TheorticalNodeLimit=781,440,243. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]===    Tepid Blanket
Puzzle:              Tepid Blanket
Ident:               SQ1~P25
Rating:              74
Size:                (12,14)
Known-Size-Solution: 280,350,383
~~~#######~~
~###.....##~
~#...###..#~
~#......#.#~
###X#P..#.#~
#...#####.#~
#...#..$O.#~
##XX#..$O##~
~#.....$OO#~
~####.#OOO##
~~~~#.#XXX.#
~~~~#...X..#
~~~~#####..#
~~~~~~~~####
CMP NodeStruct:                                                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator->SolverCoordinator:                              LS-v1.2--Forward+Reverse+SingleThread WithPeek
CMP INodeHeap->NodeHeap:                                               BlockSize: 100000
CMP INodeBacklog->NodeBacklog:                                         BlockSize: 100000
CMP ILNodeLookup->LNodeLookupBlackRedTree:
CMP ILNodeStructEvaluator->LNodeStructEvaluatorForwardDeadChecks(fwd): v1.4:Dead
CMP ILNodeStructEvaluator->LNodeStructEvaluatorReverse(rev):           v0.1
CMP INodeHashCalculator:                                               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
CMP ISolverCoordinatorFactory->SolverCoordinatorFactory:               Dead
>> Eval:21,880,000 Heap:28,601,834[Gl 10%] Lookup(128.0mil count:28,601,834 colz:214) Backlog: 6,721,833( -205)~100%  9.4min
Completed:   00:09:24.4481692
Memory used: 4751MB
Total nodes: 21,887,522 at 38,776.9nodes/sec
Dead:        13,156,428
Result:      SOLUTION!
===[FOOTER]===
Puzzle  | Rating | Time(sec) | Nodes    | Solutions | Machine | Version                               |
SQ1~P25 | 74     | 564.4     | 21887522 | 4         | willow  | LS-v1.2--Forward+Reverse+SingleThread |


----------------[attempt max]----------------------------------
PID: 18098 > ./sokosolve.pid
willow 'Intel(R) Core(TM) i7-3930K CPU @ 3.20GHz' OS:Unix6.8.0.101 dotnet:10.0.3 Threads:12 x64 RELEASE
GIT-LOG1: commit 874c90a3e0f86d73462c335eec37283eb86f8cc3
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Thu Mar 19 11:22:19 2026 +0000
GIT-LOG1:     misc: Makefile attempt for console
MemTotal:       65782612 kB
MemFree:        61847060 kB
MemAvailable:   63999480 kB
SwapCached:            0 kB
SwapTotal:       8388604 kB
SwapFree:        8388604 kB
sizeof(NodeStruct)=84. TheorticalNodeLimit=780,184,137. sizeof(NodeStructWord)=2
Available Puzzles: 1
===[Body]===    Grim Town
Puzzle:       Grim Town
Ident:        SQ1~P5
Rating:       76
Size:         (16,14)
Best-Attempt: 755,091,605
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
CMP NodeStruct:                                                        v1.1:Nested-MyBitmapStruct,CustomFloodFill
CMP SolverCoordinator->SolverCoordinator:                              LS-v1.2--Forward+Reverse+SingleThread WithPeek
CMP INodeHeap->NodeHeap:                                               BlockSize: 10000000
CMP INodeBacklog->NodeBacklog:                                         BlockSize: 1000000
CMP ILNodeLookup->LNodeLookupCompound:                                 Sharing[16] -> Dynamic=LNodeLookupBlackRedTree(312500), Immutable[LNodeLookupImmutable]
CMP ILNodeStructEvaluator->LNodeStructEvaluatorForwardDeadChecks(fwd): v1.4:Dead
CMP ILNodeStructEvaluator->LNodeStructEvaluatorReverse(rev):           v0.1
CMP INodeHashCalculator:                                               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
CMP ISolverCoordinatorFactory->SolverCoordinatorFactory:               Dead,MEMORY,VERYLARGE
>> Eval:501,430,000 Heap:552,152,539 Backlog:50,722,538(-1056)~100%  5.2hr   q
Completed:   05:10:59.2592271
Memory used: 49484MB
Total nodes: 501,430,000 at 26,873.0nodes/sec
Dead:        607,020,821
Result:      FAILED
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes     | Solutions | Machine | Version                               |
SQ1~P5 | 76     | 18659.3   | 501430000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |


----------------------------[ How many 40+ puzzles can we solve in under 10min ]---------------------------------
===[FOOTER]===
Puzzle   | Rating | Time(sec) | Nodes    | Solutions | Machine | Version                               |
SQ1~P29  | 40     | 1.1       | 77110    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P223 | 40     | 0.0       | 1643     | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P39  | 41     | 2.4       | 188858   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P23  | 41     | 0.9       | 68434    | 4         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P25  | 41     | 0.2       | 10114    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P23  | 42     | 0.2       | 13554    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P189  | 42     | 0.1       | 319      | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P237 | 42     | 6.0       | 391877   | 5         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P241 | 42     | 3.6       | 238182   | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P249 | 42     | 4.2       | 319283   | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P277  | 43     | 16.5      | 1164628  | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P235 | 44     | 22.9      | 1433043  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P41  | 45     | 1.7       | 165045   | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P7   | 45     | 0.6       | 31144    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P37  | 45     | 2.3       | 182739   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P11  | 45     | 2.5       | 303319   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P133 | 45     | 0.2       | 8871     | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P155 | 45     | 0.3       | 38894    | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P215 | 46     | 0.9       | 39163    | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P51  | 47     | 20.9      | 1412597  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P13  | 48     | 0.8       | 89376    | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P15  | 49     | 0.2       | 9617     | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P21  | 50     | 0.1       | 2086     | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P153 | 51     | 0.2       | 5708     | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P207 | 51     | 148.2     | 6584564  | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P75   | 51     | 300.2     | 12880000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P41  | 52     | 12.4      | 690384   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P45  | 52     | 300.2     | 18140000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P239 | 52     | 114.9     | 5430301  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P33   | 54     | 300.3     | 17200000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P15  | 55     | 1.7       | 170543   | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P35  | 55     | 23.0      | 1340991  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P213  | 55     | 0.1       | 266      | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P131 | 55     | 18.1      | 724377   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P209  | 56     | 41.5      | 1794735  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P31  | 57     | 87.6      | 4089580  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P225 | 57     | 0.3       | 9995     | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P53  | 59     | 300.2     | 14540000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P7   | 60     | 69.1      | 4737089  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P185  | 60     | 300.2     | 10400000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P251 | 60     | 300.2     | 16480000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P47  | 61     | 300.3     | 15010000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P9   | 63     | 1.5       | 137687   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P5   | 64     | 255.3     | 14423303 | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P43  | 64     | 63.7      | 5578979  | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P135 | 65     | 0.1       | 263      | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P151 | 65     | 1.2       | 54114    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P43  | 66     | 12.6      | 765031   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P227 | 66     | 180.1     | 6074772  | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P9   | 67     | 247.5     | 9903218  | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P57  | 67     | 33.3      | 1462082  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P43  | 68     | 300.4     | 12440000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P291  | 69     | 31.5      | 513934   | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P149 | 69     | 1.8       | 142101   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P15  | 70     | 300.1     | 15170000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P147 | 71     | 4.0       | 150153   | 4         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P11   | 71     | 300.3     | 12300000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P289  | 72     | 9.6       | 329967   | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P217 | 72     | 300.4     | 6440000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P305  | 73     | 300.2     | 18290000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P145 | 73     | 0.9       | 16428    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P25  | 74     | 300.1     | 12730000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P5   | 76     | 300.4     | 10040000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P39  | 77     | 8.5       | 630012   | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P3    | 77     | 300.3     | 14150000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P47  | 78     | 300.1     | 10980000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P247 | 79     | 33.6      | 1586029  | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P3   | 80     | 300.1     | 10380000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P25  | 80     | 5.7       | 393687   | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P73  | 80     | 300.3     | 11050000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P13   | 80     | 300.2     | 13570000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P63  | 82     | 293.0     | 11001051 | 4         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P11  | 83     | 300.2     | 11720000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P29  | 83     | 228.4     | 7577076  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P13  | 87     | 1.2       | 56011    | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P153  | 87     | 300.4     | 10290000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P143 | 92     | 0.3       | 5572     | 3         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P23  | 93     | 300.3     | 9640000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P97   | 100    | 300.3     | 11040000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P47  | 102    | 300.3     | 7170000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P51   | 106    | 300.1     | 12380000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P27  | 107    | 300.3     | 7990000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB2~P229 | 107    | 300.3     | 4450000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P101  | 107    | 300.4     | 8980000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
MB~P287  | 111    | 300.2     | 10790000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P31  | 112    | 240.4     | 5642947  | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P63   | 114    | 300.1     | 7900000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P59  | 115    | 300.6     | 6610000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P167  | 115    | 300.2     | 6810000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P41   | 117    | 300.2     | 8400000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P45  | 119    | 300.2     | 8730000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P173  | 119    | 300.1     | 7930000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P27  | 121    | 300.4     | 6790000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P31   | 122    | 300.3     | 12350000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P67   | 124    | 300.4     | 11250000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P17  | 128    | 300.5     | 5500000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P25  | 128    | 300.1     | 8420000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P91   | 128    | 300.5     | 8300000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P65  | 131    | 9.3       | 385688   | 2         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P65   | 132    | 300.4     | 10460000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P55  | 133    | 300.1     | 10380000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P29  | 140    | 300.3     | 6350000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P67  | 144    | 300.2     | 4330000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P89   | 153    | 300.3     | 10460000 | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P67  | 156    | 300.4     | 6480000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P81  | 160    | 300.2     | 4400000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P35  | 163    | 300.3     | 4420000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P75  | 166    | 300.5     | 6070000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P15   | 172    | 300.2     | 6170000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P39  | 174    | 300.1     | 4270000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P61  | 193    | 18.6      | 443397   | 1         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P73  | 199    | 300.3     | 6670000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P41  | 214    | 300.3     | 8120000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P71  | 215    | 301.0     | 2110000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P31  | 233    | 300.8     | 4920000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P35  | 246    | 300.7     | 5590000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P37  | 246    | 300.5     | 2270000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
TR~P95   | 263    | 300.3     | 3740000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ1~P9   | 290    | 300.3     | 3750000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ5~P69  | 323    | 301.0     | 2290000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ3~P45  | 328    | 300.1     | 5410000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |
SQ4~P33  | 543    | 301.3     | 1280000  | 0         | willow  | LS-v1.2--Forward+Reverse+SingleThread |

