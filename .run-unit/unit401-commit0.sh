cd /c/Source/HamLet
sed "s/$/\r/" .run-unit/unit401-outcome.txt >> PHASE_OUTCOME.md
tail -c 200 PHASE_OUTCOME.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
sh .run-unit/unit401-commit.sh "unit401 task 0: entry round - decision 6 diff empty, lines are unit 400 exit runs, app 277 of 278 twice on dispatcher losses, engine 176 of 176 in 374 s; floors 37 of 37 identical to unit 400 exit, 13 of 13, synthetics 2 of 2; CwFixtureTests 22 of 23, CwDisplacementFloorTests 0 of 6, CwEmissionGateTests 7 of 8; version 1.13.88; PHASE_STATUS was CURRENT_STEP 0 and 400, now 3 and 401" "TASK 1 of 5" "task 0 committed; task 1: writing the four-ways printer over the six displacement cases and #24" PHASE_OUTCOME.md PHASE_STATUS.md Directory.Build.props docs/phase-cw/unit401-closeout.md
printf 'TASK 1 of 5\n' > .run-unit/unit401-task.txt
date +%H:%M:%S
