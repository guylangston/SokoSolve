dot:
	dotnet run -- solve --puzzle SQ1~P1 --tags graphviz
	dot -O -Tpng ./sokosolve-tree.dot

dot-firefox: dot
	firefox ./sokosolve-tree.dot.png

build:
	@dotnet build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly --no-restore -c Release

attempt: build
	dotnet run -c Release --no-build -- --pid solve --puzzle SQ1~P5 --veryLarge --tags Dead
