#!/bin/sh
# unit 477 - close: keep a copy of the report, write the finished status, commit and push task 4.
cd /c/Source/HamLet || exit 1
cp output.md .run-unit/unit477-output.md || exit 1
sh .run-unit/unit477-status.sh COMPLETED "4 of 4" tim "output.md -> Claude Web" "Unit 477 done: the scope finds keying by bars (flat, dropped, flat again) with no floor or margin; the tracker takes the meter's pitch and keying flag on its next hop; task 3 dropped, meter still gates on 17 dB; 476's 15 dB test left red (51/208); no recording read; launcher's STOP and SESSION.lock left in place" > /dev/null || exit 1
sh .run-unit/unit477-commit.sh .run-unit/unit477-msg4.txt output.md 2>&1 | grep -v "^warning" | tail -4
head -10 PROJECT_STATUS.md
