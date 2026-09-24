#!/bin/sh
# unit 424 task 4 - final status, commit the exit round and output.md, push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "Unit 424 complete and pushed: CW preamp from the IC-7300 manual, preamp 1 on HF, 2 at 50 MHz, off when overloading at tune-in; contradicting sentences 28 to 0; 7.8 left unticked for P22; output.md written"
git add -- output.md PROJECT_STATUS.md docs/phase-correctness/PARKED.md .run-unit/unit424-*.txt .run-unit/unit424-*.sh
git commit -q -F .run-unit/unit424-msg-t4.txt
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short
