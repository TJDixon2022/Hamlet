#!/bin/sh
# unit 423 - final status, commit task 4 with the run logs, push, status again from the clock.
cd /c/Source/HamLet || exit 1
NOTE="Unit 423 complete: 6.3 ticked - tuned outside his privileges the card, sun map and rig face keep their columns at every size, words and color still change; row height parked as P21; P16 and P20 red as at entry"
sh tools/status.sh COMPLETED "TASK 4 of 4" tim "output.md -> Claude Web" "$NOTE"
git add -- output.md PHASE_PLAN.md PHASE_OUTCOME.md PROJECT_STATUS.md .run-unit/unit423-* || exit 1
git commit -q -F .run-unit/unit423-msg-t4.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short -- src tests docs output.md PHASE_PLAN.md PHASE_OUTCOME.md
git log --oneline origin/main -1
