#!/bin/sh
# unit 443 - tick 3.1 in both plan copies, and show what src carries against entry.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit443-tick.sh 3.1
echo "src against entry 1308f688:"
git diff --stat 1308f688 -- src
echo "(end of src diff)"
git diff --stat 1308f688 -- tests | tail -3
date "+%Y-%m-%d %H:%M"
