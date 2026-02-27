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
