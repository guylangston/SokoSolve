param([string[]]$args)
& ./touch-git.ps1
dotnet build -c Release -f netcoreapp31 -p:WarningLevel=0 
dotnet run   -c Release -f netcoreapp31 --no-build -- benchmark $args