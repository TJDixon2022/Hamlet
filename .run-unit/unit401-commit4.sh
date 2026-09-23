cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
git diff --stat -- docs
sh .run-unit/unit401-commit.sh "unit401 task 3: the known-reds block names no CW test, the failing set closed out 31/12/0/8, the third floor test on the engine line 178 of 178 in 374 s; 3.4" "TASK 3 of 5" "task 3: close-out committed; writing the 3.4 tick, 2.3 and 3.1 clauses" docs/carry-forward-tests.txt docs/unit239-failing-set.txt docs/phase-cw/unit401-closeout.md
date +%H:%M:%S
grep -nE "^\s*- \[.\] \*?\*?(2\.3|3\.1|3\.4|3\.5)" PHASE_PLAN.md | cut -c1-120
