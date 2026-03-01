PID: 6916
FIG 'AMD64 Family 25 Model 33 Stepping 2, AuthenticAMD' OS:WIN10.0.26200.0 dotnet:10.0.3 Threads:16 x64 RELEASE
GIT-LOG1: commit 3910d2771dc3364bc514860e532108103356da9f
GIT-LOG1: Author: Guy Langston <guylangston@gmail.com>
GIT-LOG1: Date:   Fri Feb 27 15:00:48 2026 +0000
GIT-LOG1:     fix: friendly name; feat: NodeStruct as a component
GIT-STAT: On branch master
GIT-STAT: Your branch is up to date with 'origin/master'.
GIT-STAT: nothing to commit, working tree clean
sizeof(NodeStruct)=88. TheorticalNodeLimit=0. sizeof(NodeStructWord)=2
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
CMP SolverCoordinator:                 LS-v1.1--ForwardOnly+SingleThread WithPeek
CMP NodeHeap:                          BlockSize: 100000
CMP INodeBacklog:                      SokoSolve.LargeSearchSolver.NodeBacklog
CMP ILNodeLookup:                      SokoSolve.LargeSearchSolver.Lookup.LNodeLookupBlackRedTree
CMP LNodeStructEvaluatorForwardStable: DropVectorInt2
CMP INodeHashCalculator:               SokoSolve.LargeSearchSolver.NodeHashSytemHashCode
>> Eval:15,520,000 Heap:15,522,823 Backlog: [G 100%]      2,823(-2838) 1%  Lookup(62.3mil count:15,522,824 col:28) 2.0min
Completed:   00:02:02.1196667
Memory used: 2624MB
Total nodes: 15,529,013 at 127,162.3nodes/sec
Result:      SOLUTION!(1)
===[FOOTER]===
Puzzle | Rating | Time(sec) | Nodes    | Solutions | Machine | Version                           |
SQ1~P7 | 60     | 122.1     | 15529013 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |



98b2ca51-d20f-4fc3-9aac-aa2f0173c31a~P11 | 45     | 4.9       | 1209554  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P133                                 | 45     | 2.9       | 315698   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P155                                 | 45     | 0.9       | 276083   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P215                                 | 46     | 24.9      | 1994519  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
SQ4~P51                                  | 47     | 19.6      | 2710290  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
98b2ca51-d20f-4fc3-9aac-aa2f0173c31a~P13 | 48     | 11.6      | 2153963  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
SQ1~P15                                  | 49     | 1.3       | 260656   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
98b2ca51-d20f-4fc3-9aac-aa2f0173c31a~P21 | 50     | 0.7       | 125049   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P153                                 | 51     | 2.1       | 263279   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P207                                 | 51     | 166.0     | 14293169 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
TR~P75                                   | 51     | 1200.0    | 74320000 | 0         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
SQ4~P41                                  | 52     | 97.5      | 9218557  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
SQ4~P45                                  | 52     | 470.8     | 44538287 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P239                                 | 52     | 109.0     | 9462144  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
TR~P33                                   | 54     | 357.1     | 36608906 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
98b2ca51-d20f-4fc3-9aac-aa2f0173c31a~P15 | 55     | 15.8      | 2511036  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
98b2ca51-d20f-4fc3-9aac-aa2f0173c31a~P35 | 55     | 64.8      | 7047047  | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB~P213                                  | 55     | 0.0       | 3095     | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P131                                 | 55     | 583.4     | 37154977 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB~P209                                  | 56     | 3.9       | 708740   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
98b2ca51-d20f-4fc3-9aac-aa2f0173c31a~P31 | 57     | 540.2     | 38498464 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P225                                 | 57     | 2.0       | 301381   | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
SQ4~P53                                  | 59     | 1063.4    | 77482712 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
SQ1~P7                                   | 60     | 150.6     | 15529013 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB~P185                                  | 60     | 1154.0    | 73528528 | 1         | FIG     | LS-v1.1--ForwardOnly+SingleThread |
MB2~P251                                 | 60     | 1024.7    | 63790000 | 0         | FIG     | LS-v1.1--ForwardOnly+SingleThread |


