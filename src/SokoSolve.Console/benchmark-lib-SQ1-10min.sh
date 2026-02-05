#!/bin/sh
echo " | Ctrl+C to exit (will gracefully exit with report)"
dotnet run -c Release -- benchmark SQ1 --pool bb:lock:bst:lt --min 10
