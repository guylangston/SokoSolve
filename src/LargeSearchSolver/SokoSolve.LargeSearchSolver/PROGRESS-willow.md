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
