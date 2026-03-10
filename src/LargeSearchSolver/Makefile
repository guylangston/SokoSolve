default: clean restore test micro bench

clean:
	@dotnet clean -c Release
	@dotnet clean -c Debug

restore:
	@dotnet restore

build:
	@dotnet build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly --no-restore

build-release:
	@dotnet build --nologo -v q --property WarningLevel=0 /clp:ErrorsOnly -c Release --no-restore

test: build
	@ dotnet test --no-build -l "console;verbosity=detailed" --blame-hang --blame-hang-timeout 20sec

micro: build-release
	@cd ./SokoSolve.LargeSearchSolver.MicroBenchmarks/; dotnet run -c Release

bench: build-release
	@echo "BENCHMARK"
	@cd ./SokoSolve.LargeSearchSolver.Console/; dotnet run -c Release --  solve --puzzle SQ1~P7

attempt: build-release
	@cd ./SokoSolve.LargeSearchSolver.Console/; dotnet run -c Release -- --pid solve --puzzle __target --veryLarge


