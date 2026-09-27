#!/bin/sh
# unit 465 task 4 - run the exit prints and keep them.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit465-exit-print.sh > .run-unit/unit465-exit-print.txt 2>&1
grep -v "^warning" .run-unit/unit465-exit-print.txt | cut -c1-230
