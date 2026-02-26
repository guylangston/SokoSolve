#!/bin/bash
tfm="net10.0"
dotnet build -c Release -f $tfm -p:WarningLevel=0 -v q
dotnet run   -c Release -f $tfm --no-build -- benchmark "$@"
echo "dotnet run -c Release -f $tfm -- benchmark $@"


