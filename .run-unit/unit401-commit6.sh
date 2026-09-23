cd /c/Source/HamLet
sed "s/$/\r/" .run-unit/unit401-outcome2.txt >> PHASE_OUTCOME.md
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
sh .run-unit/unit401-transmit.sh
sh .run-unit/unit401-commit.sh "unit401 task 4: exit round - app 278 of 278 in 156 s, engine 178 of 178 in 374 s with the clean synthetics on it; floors 37 of 37 every row identical to entry, 13 of 13, 2 of 2; CwFixtureTests 22 of 23, CwDisplacementFloorTests 6 of 6, CwEmissionGateTests 7 of 8; transmit files and src unchanged; no regression, 3.5 stands; PARKED 401 items 1 to 3" "TASK 4 of 5" "task 4 committed, no regression; writing output.md" PHASE_OUTCOME.md docs/phase-cw/PARKED.md docs/phase-cw/unit401-closeout.md
echo "tests docs vs bf7bae57:"; git diff --stat bf7bae57 HEAD -- tests docs; echo end
date +%H:%M:%S
