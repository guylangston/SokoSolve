build:
	@dotnet build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly

build-release:
	@dotnet build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly -c Release

test: build
	@dotnet test --no-restore

micro: build-release
	@cd ./SokoSolve.LargeSearchSolver.MicroBenchmarks/; dotnet run -c Release

bench: build-release
	@cd ./SokoSolve.LargeSearchSolver.Console/; dotnet run -c Release --  solve --puzzle SQ1~P7

attempt: build-release
	@cd ./SokoSolve.LargeSearchSolver.Console/; dotnet run -c Release -- --pid solve --puzzle __target --veryLarge

full: test micro bench
