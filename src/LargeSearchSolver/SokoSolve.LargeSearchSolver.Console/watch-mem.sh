#!/bin/sh
PID="${A:-$(cat ./sokosolve.pid)}"
cat "/proc/$PID/status"
if command -v pidstat; then
        pidstat -r -p "$PID" 30
else
    echo "https://man.archlinux.org/man/extra/sysstat/pidstat.1.en"
    echo "pidstat not found. consider pacman -S sysstat"
fi
