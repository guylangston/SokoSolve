param([string[]]$args)
& ./touch-git.ps1
$tfm = "netcoreapp3.1"
dotnet build -c Release -f $tfm -p:WarningLevel=0 
dotnet run   -c Release -f $tfm --no-build -- benchmark $args

# Examples for copy/past
# dotnet run -c Release -f netcoreapp31 -- benchmark
# dotnet run -c Release -f netcoreapp31 -- benchmark --solver fr!,fr!p --pool bb:lock:bst:lt,bb:lock:ll:lt --min 3
# dotnet run -c Release -f netcoreapp31 -- benchmark SQ1 --solver fr!,fr!p --pool bb:lock:bst:lt,bb:lock:ll:lt --min 3 --min-rating 80 --max-rating 1000