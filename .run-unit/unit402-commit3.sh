cd /c/Source/HamLet
echo "failing set diff:"; git diff --stat -- docs/unit239-failing-set.txt; git diff -U0 -- docs/unit239-failing-set.txt | grep -E "^@@|^[-+]" | cut -c1-120
sh .run-unit/unit402-transmit.sh
echo "app:"; git diff --stat 5688a8a5 HEAD -- src/Hamlet.App; echo end
sh .run-unit/unit402-commit.sh "unit402 task 3: 3.6 attempt 1 on the eight, 2 green, 6 red-open, 6 attacked - #24 #41 green at 7e65aac4; #42 green on its own type but put back for two captures and VA3VRR; #43 to #45 moved on the settled event and put back; #6 #15 not attacked, no cause at a line; reds-3.6.md opened; closing line 45 green, 6 red-open; 3.1 clause; PARKED 401 item 1 struck, 402 items 1 and 2" "TASK 4 of 5" "task 3 committed; task 4 exit round: app carry-forward line running" docs/phase-cw/reds-3.6.md docs/phase-cw/unit402-reds.md docs/phase-cw/PARKED.md docs/unit239-failing-set.txt PHASE_PLAN.md
date +%H:%M:%S
