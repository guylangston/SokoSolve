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
