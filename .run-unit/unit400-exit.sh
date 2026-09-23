cd /c/Source/HamLet
sh .run-unit/unit400-floors.sh floors-exit-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 4 of 5" "task 4: exit round, both lines done with no assertion red; captures type running" --no-build
sh .run-unit/unit400-floors.sh floors-exit-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 4 of 5" "task 4: exit round, adjudicated type running" --no-build
sh .run-unit/unit400-floors.sh floors-exit-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 4 of 5" "task 4: exit round, clean synthetics running" --no-build
echo "captures against entry:"; sh .run-unit/unit400-cmp.sh .run-unit/unit400-floors-1.txt .run-unit/unit400-floors-exit-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit400-adjcmp.sh .run-unit/unit400-floors-2.txt .run-unit/unit400-floors-exit-2.txt
sh .run-unit/unit400-readers.sh exit "TASK 4 of 5" "task 4: exit round, floors done;" > .run-unit/unit400-readers-exit.txt 2>&1
grep -E "^exit|Total tests|Passed:|Failed:" .run-unit/unit400-readers-exit.txt
grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-fixtures-prosigns.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-fx-a.txt
grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-fixtures-exit.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-fx-b.txt
echo "CwFixtureTests task 3 against exit:"; diff .run-unit/unit400-fx-a.txt .run-unit/unit400-fx-b.txt; echo "diff rc $?"
grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-cleanreads-exit.txt .run-unit/unit400-survey-exit.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-cr-d.txt
echo "clean reads and survey entry against exit:"; grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-cleanreads-entry.txt .run-unit/unit400-survey-entry.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-cr-e.txt; diff .run-unit/unit400-cr-e.txt .run-unit/unit400-cr-d.txt; echo "diff rc $?"
sh .run-unit/unit400-callers.sh exit "TASK 4 of 5" "task 4: exit round, caller" > .run-unit/unit400-callers-exit-all.txt 2>&1
for T in CwAcquisitionWindowTests CwSensitivityTests EveryCharacterCarriesItsOwnEvidenceTests WhereAcquisitionPointsTests CwRefusalFloorTableTests; do
  grep -E "^\s+(Passed|Failed) " .run-unit/unit400-callers-entry-$T.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-ca.txt
  grep -E "^\s+(Passed|Failed) " .run-unit/unit400-callers-exit-$T.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-cb.txt
  echo "$T entry $(grep -c ^Passed .run-unit/unit400-ca.txt) green $(grep -c ^Failed .run-unit/unit400-ca.txt) red, exit $(grep -c ^Passed .run-unit/unit400-cb.txt) green $(grep -c ^Failed .run-unit/unit400-cb.txt) red"; diff .run-unit/unit400-ca.txt .run-unit/unit400-cb.txt; echo "diff rc $?"
done
sh .run-unit/unit400-transmit.sh
echo "src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo end
echo "tests vs 0aa08d32:"; git diff --stat 0aa08d32 HEAD -- tests; echo end
echo "status tests:"; git status --short -- tests; echo end
echo "ANALYSIS pages untracked or modified:"; git status --short | grep ANALYSIS; echo end
echo "carry list and set vs 0aa08d32:"; git diff --stat 0aa08d32 HEAD -- docs/carry-forward-tests.txt docs/unit239-failing-set.txt docs/cw-retired-tests.txt; echo end
echo "worktrees:"; git worktree list
date +%H:%M:%S
