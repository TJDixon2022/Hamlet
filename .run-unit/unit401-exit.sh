cd /c/Source/HamLet
sh .run-unit/unit401-floors.sh floors-exit-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 4 of 5" "task 4: both lines green, app 278 engine 178; captures type running" --no-build
sh .run-unit/unit401-floors.sh floors-exit-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 4 of 5" "task 4: exit round, adjudicated type running" --no-build
sh .run-unit/unit401-floors.sh floors-exit-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 4 of 5" "task 4: exit round, clean synthetics running" --no-build
sh .run-unit/unit401-types.sh exit "TASK 4 of 5" "task 4: exit round, floors done;" fixtures disp gate
echo "captures against entry:"; sh .run-unit/unit401-cmp.sh .run-unit/unit401-floors-1.txt .run-unit/unit401-floors-exit-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit401-adjcmp.sh .run-unit/unit401-floors-2.txt .run-unit/unit401-floors-exit-2.txt
echo "fixtures against entry:"; diff .run-unit/unit401-fixtures-entry-list.txt .run-unit/unit401-fixtures-exit-list.txt; echo "diff rc $?"
echo "gate against entry:"; diff .run-unit/unit401-gate-entry-list.txt .run-unit/unit401-gate-exit-list.txt; echo "diff rc $?"
sh .run-unit/unit401-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
echo "tests docs vs bf7bae57:"; git diff --stat bf7bae57 HEAD -- tests docs; echo end
echo "status tests:"; git status --short -- tests; echo end
echo "grep Cw CW carry:"; grep -n "Cw\|CW" docs/carry-forward-tests.txt | cut -c1-100
echo "worktrees:"; git worktree list
date +%H:%M:%S
