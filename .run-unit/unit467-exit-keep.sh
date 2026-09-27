#!/bin/sh
# unit 467 task 2 - keep the exit prints under .run-unit and show them.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit467-exit-print.sh > .run-unit/unit467-exit-print.txt 2>&1
cut -c1-200 .run-unit/unit467-exit-print.txt
