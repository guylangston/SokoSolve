#!/bin/bash
puzzle="${1:-SQ1}"
solver="${2:-fr!}"
pool="${3:-bb:bst:lt}"
min="${4:-3}"
args="$puzzle --solver $solver --pool $pool --min $min"
tfm="netcoreapp5.0"
dotnet build -c Release -f $tfm -p:WarningLevel=0 
dotnet run   -c Release -f $tfm --no-build -- benchmark $args
echo "dotnet run -c Release -f $tfm -- benchmark $args"


