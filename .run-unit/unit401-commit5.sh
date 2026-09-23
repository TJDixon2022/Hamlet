cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
git diff --stat -- PHASE_PLAN.md docs
sh .run-unit/unit401-commit.sh "unit401 ticks: 3.4 ticked on its letter, the step partial on 8 red-open under R49; 2.3 gains the clean synthetics on the engine line, 178 of 178 in 374 s; 3.1 gains #18 to #23 green on the settled transcript and a band of 0.005, the set at 43 green and 8 red-open; doc section 6" "TASK 4 of 5" "task 3 committed with ticks; task 4: exit round, app line running first" PHASE_PLAN.md docs/phase-cw/unit401-closeout.md
printf 'TASK 4 of 5\n' > .run-unit/unit401-task.txt
date +%H:%M:%S
