#!/usr/bin/env pwsh
param([string[]]$args)
pushd
cd src/SokoSolve.Console
. ./benchmark.ps1 $args
popd
