#!/usr/bin/env pwsh
param($puzzle)
& ./touch-git.ps1

$dotnetf = "net9.0"

# dotnet build -c Release -f "netcoreapp3.1" -p:WarningLevel=0
# dotnet run   -c Release -f "netcoreapp3.1" --no-build -- benchmark $puzzle

dotnet build -c Release -f "$dotnetf" -p:WarningLevel=0
dotnet run   -c Release -f "$dotnetf" --no-build -- benchmark $puzzle

# Examples for copy/past
# dotnet run -c Release -f $tfm -- benchmark
# dotnet run -c Release -f $tfm -- benchmark --solver fr!,fr!p --pool bb:lock:bst:lt,bb:lock:ll:lt --min 3
# dotnet run -c Release -f $tfm -- benchmark SQ1 --solver fr!,fr!p --pool bb:lock:bst:lt,bb:lock:ll:lt --min 3 --min-rating 80 --max-rating 1000
