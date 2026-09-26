#!/bin/sh
# unit 444 - task 1 commit: the gap printer, both printouts, the line-up and the per-recording comparison.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit444-v11.sh .run-unit/unit444-metrics-f14b2453.txt .run-unit/unit444-metrics-entry.txt > .run-unit/unit444-g1-per-recording.txt
tail -1 .run-unit/unit444-g1-per-recording.txt
sh tools/status.sh EXECUTING "1 of 3" code none "Trace done: G1 lost 6-R and W-B, letter spaces of 310 and 320 ms; a 10-15 ms dropout makes G1 fire; committing"
sh .run-unit/unit444-commit.sh .run-unit/unit444-msg1.txt PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/HowSeventeenThirtySevensGapsAreCalledTests.cs .run-unit/unit444-*.txt .run-unit/unit444-*.sh .run-unit/unit444-*.md
