cd /c/Source/HamLet
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
git diff --stat -- tests
sh .run-unit/unit401-commit.sh "step 3 repair: CwDisplacementFloorTests generates over a band of 0.005 under HM-DEC-127, 6 of 6, R49 - the five silent cases, the smallest band the printer found; captures 37 of 37 identical, adjudicated 13 of 13, CwFixtureTests 22 of 23 and CwEmissionGateTests 7 of 8 identical by case" "TASK 3 of 5" "task 2 committed, displacement 6 of 6; task 3: editing the known-reds block and the failing set closing line" tests/Hamlet.RadioEngine.Tests/Cw/CwDisplacementFloorTests.cs docs/phase-cw/unit401-closeout.md
printf 'TASK 3 of 5\n' > .run-unit/unit401-task.txt
date +%H:%M:%S
