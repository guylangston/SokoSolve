param([string]$args)
& ./touch-git.ps1
dotnet build -p:WarningLevel=0 -f netcoreapp31 
dotnet run -f netcoreapp31 --no-build -- benchmark $args
