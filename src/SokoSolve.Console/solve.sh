#!/bin/bash
puzzle="${1:-SQ1}"
args="$puzzle --solver fr! --pool bb:lock:bst:lt --min 5"
tfm="netcoreapp5.0"
dotnet build -c Release -f $tfm -p:WarningLevel=0 
dotnet run   -c Release -f $tfm --no-build -- benchmark $args
echo "dotnet run -c Release -f $tfm -- benchmark $args"


