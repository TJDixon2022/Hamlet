#!/bin/sh
# unit 436 - task 3's commit with the final status, then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh COMPLETED "TASK 3 of 3" tim "output.md -> Claude Web" "Unit 436 complete and pushed: the dah-over-3 rule read the opening at 18 to 22 WPM but cost 4 keyed floors and 14 capture rows, so it came back out; the pitch still breaks the opening; 7.4 not ticked; output.md written"
git add -- output.md PROJECT_STATUS.md docs/phase-correctness/PARKED.md .run-unit/unit436-* || exit 1
git commit -q -F .run-unit/unit436-msg-t3.txt || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
git status --short | grep -v "^??"
