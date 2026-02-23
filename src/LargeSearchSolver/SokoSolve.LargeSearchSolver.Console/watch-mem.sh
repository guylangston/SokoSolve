#!/bin/sh
if command -v pidstat; then
    pidstat -r -p "$(cat ./sokosolve.pid)" 10 100
else
    echo "https://man.archlinux.org/man/extra/sysstat/pidstat.1.en"
    echo "pidstat not found. consider pacman -S sysstat"
fi
