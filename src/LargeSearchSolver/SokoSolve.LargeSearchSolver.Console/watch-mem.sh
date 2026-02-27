#!/bin/sh
if command -v pidstat; then
    if [ -n "$1" ]; then
        pidstat -r -p "$1" 10 100
    else
        pidstat -r -p "$(cat ./sokosolve.pid)" 10 100
    fi
else
    echo "https://man.archlinux.org/man/extra/sysstat/pidstat.1.en"
    echo "pidstat not found. consider pacman -S sysstat"
fi
