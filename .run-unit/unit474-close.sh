#!/bin/sh
# unit 474 - close: keep a copy of the report, write the finished status, commit and push task 4.
cd /c/Source/HamLet || exit 1
cp output.md .run-unit/unit474-output.md || exit 1
sh .run-unit/unit474-status.sh COMPLETED "4 of 4" tim "output.md -> Claude Web" "Unit 474 done: the CW tab has a light, a pitch strip and two verdict buttons that write owner_verdict rows; no recording read; an owner STOP file is at the root" > /dev/null || exit 1
sh .run-unit/unit474-commit.sh .run-unit/unit474-msg4.txt output.md 2>&1 | grep -v "^warning" | tail -8
head -10 PROJECT_STATUS.md
