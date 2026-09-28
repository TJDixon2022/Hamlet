#!/bin/sh
# unit 476 - close: keep a copy of the report, write the finished status, commit and push task 4.
cd /c/Source/HamLet || exit 1
cp output.md .run-unit/unit476-output.md || exit 1
sh .run-unit/unit476-status.sh COMPLETED "4 of 4" tim "output.md -> Claude Web" "Unit 476 done: the oscilloscope is on the CW tab - bars light where the envelope stands 9 dB over the noise, at any pitch; 12.1 and 12.3 ticked, 12.2 left for the owner's eye; no recording read; owner STOP file still at the root" > /dev/null || exit 1
sh .run-unit/unit476-commit.sh .run-unit/unit476-msg4.txt output.md 2>&1 | grep -v "^warning" | tail -8
head -10 PROJECT_STATUS.md
