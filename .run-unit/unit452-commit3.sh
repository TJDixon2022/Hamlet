#!/bin/sh
# unit 452 - the exit commit: printouts, report, status.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "3 of 3" code none "Unit 452 done: 6.3 ticked, WBE real 46 to 37 kept with letters identical, 17:37 still 7 - output.md written"
sh .run-unit/unit452-commit.sh .run-unit/unit452-msg3.txt output.md PROJECT_STATUS.md .run-unit/unit452-*.txt .run-unit/unit452-*.sh .run-unit/unit452-*.md
git status -sb | head -1
