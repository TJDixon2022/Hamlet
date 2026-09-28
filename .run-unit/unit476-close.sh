#!/bin/sh
# unit 476 - close: keep a copy of the report, write the finished status, commit and push task 3.
cd /c/Source/HamLet || exit 1
cp output.md .run-unit/unit476-output.md || exit 1
sh .run-unit/unit476-status.sh COMPLETED "3 of 3" tim "output.md -> Claude Web" "Unit 475 done: swing bar 20 to 17, red at 20 and green at 17 on a synthetic window; 17 sits below the 17.7 empty recording - ruling asked; no recording read; owner STOP file still at the root" > /dev/null || exit 1
sh .run-unit/unit476-commit.sh .run-unit/unit476-msg3.txt output.md 2>&1 | grep -v "^warning" | tail -8
head -10 PROJECT_STATUS.md
