dot:
	dotnet run -- solve --puzzle SQ1~P1 --tags graphviz
	dot -O -Tpng ./sokosolve-tree.dot

dot-firefox: dot
	firefox ./sokosolve-tree.dot.png

build:
	@dotnet build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly --no-restore -c Release

attempt: build
	dotnet run -c Release --no-build -- --pid solve --puzzle SQ1~P5 --veryLarge --stop-on-swap --tags Dead

clean:
	[ -d ./dist/raw ] && rm -rf ./dist/raw || true

dist: clean build
	dotnet publish -c Release --sc -o ./dist/raw/
	cp -r ../../../data ./dist/raw

docker: dist
	cd ./dist; docker build -t sokosolve .; docker run -it sokosolve:latest solve --puzzle SQ1~P7 --non-interactive
