cd /c/Source/HamLet
sed "s/$/\r/" .run-unit/unit402-outcome.txt >> PHASE_OUTCOME.md
tail -c 200 PHASE_OUTCOME.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit402-transmit.sh
sh .run-unit/unit402-commit.sh "unit402 task 0: entry round - decision 9 diff empty, lines are unit 401 exit runs, app 278 of 278 in 156 s, engine 178 of 178 in 374 s; floors 37 of 37 identical to unit 401 exit, 13 of 13, 2 of 2; the eight red at entry, #6 0.75, #15 0.54, #24 no speed, #41 no speed, #42 70, #43 to #45 5+37, 3+21, 1+3; version 1.13.89; PHASE_STATUS was CURRENT_STEP 0 and 401, now 3 and 402" "TASK 1 of 5" "task 0 committed; task 1: reading CwDecoder guard and the four red tests before writing the eight-reds printer" PHASE_OUTCOME.md PHASE_STATUS.md Directory.Build.props docs/phase-cw/unit402-reds.md
printf 'TASK 1 of 5\n' > .run-unit/unit402-task.txt
date +%H:%M:%S
