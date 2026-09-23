cd /c/Source/HamLet
cat .run-unit/unit400-outcome.txt >> PHASE_OUTCOME.md
tail -c 200 PHASE_OUTCOME.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit400-transmit.sh
sh .run-unit/unit400-commit.sh "unit400 task 0: entry round - decision 6 diff empty, lines are unit 399 exit runs, app 278 of 278, engine 176 of 176; floors 37 of 37 identical to unit 399 exit, 13 of 13, synthetics 0 of 2 under R53; CwFixtureTests 14 green 9 red; five caller types baselined, acquisition 10 of 12, sensitivity 1 of 2 with TheDecoderReadsAsFarDownAsItDidBefore red outside the set, evidence 3 of 3, where 2 of 2, refusal 1 of 1; version 1.13.87; PHASE_STATUS was CURRENT_STEP 0 and 399, now 3 and 400" "TASK 0 of 5" "task 0 committed; task 1 next: changing harness line 71 to CharacterSettled, then building" PHASE_OUTCOME.md PHASE_STATUS.md Directory.Build.props docs/phase-cw/unit400-harness.md
echo "TASK 1 of 5" > .run-unit/unit400-task.txt
date +%H:%M:%S
