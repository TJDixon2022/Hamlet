#!/bin/sh
# unit 417 - final status, exit commit with the report, push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 3 of 3" tim "output.md -> Claude Web" "Unit 417 complete: tonePeak measured over each capture, 17:37 25.7 held to 26.8, 862 ms off the UI thread; 6.7 ticked, 6.2 held on the keying caption; 2 items parked, none blocking"
git add -- output.md PHASE_OUTCOME.md PHASE_PLAN.md PHASE_STATUS.md PROJECT_STATUS.md docs/phase-correctness/PARKED.md .run-unit/unit417-* || exit 1
git commit -q -F .run-unit/unit417-msg-t3.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short | grep -v "^ M .run-unit/\|^ D .run-unit/\|SESSION.lock"
