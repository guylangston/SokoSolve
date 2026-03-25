default: clean restore test bench

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
	@dotnet test --no-build

test-blame: build
	dotnet test --no-build -l "console;verbosity=detailed" --blame-hang --blame-hang-timeout 20sec

micro: build-release
	@cd ./SokoSolve.LargeSearchSolver.MicroBenchmarks/; dotnet run -c Release

bench: build-release
	@echo "BENCHMARK"
	@echo "[baseline] Puzzle | Rating | Time(sec) | Nodes   | Solutions | Machine     | Version                                            |"
	@echo "[baseline] SQ1~P7 | 60     | 68.4      | 4737089 | 1         | guyzen-arch | LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking |"
	@cd ./SokoSolve.LargeSearchSolver.Console/; dotnet run -c Release --  solve --puzzle SQ1~P7
	@echo "[baseline] Puzzle | Rating | Time(sec) | Nodes   | Solutions | Machine     | Version                                            |"
	@echo "[baseline] SQ1~P7 | 60     | 68.4      | 4737089 | 1         | guyzen-arch | LS-v1.4(Fwd,Rev,T1)+Peek+Debugger+SolutionTracking |"

attempt: build-release
	make -C ./SokoSolve.LargeSearchSolver.Console attempt


