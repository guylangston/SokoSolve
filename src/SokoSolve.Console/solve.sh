#!/bin/bash
tfm="netcoreapp5.0"
dotnet build -c Release -f $tfm -p:WarningLevel=0 
dotnet run   -c Release -f $tfm --no-build -- benchmark "$@" 
echo "dotnet run -c Release -f $tfm -- benchmark $@"


