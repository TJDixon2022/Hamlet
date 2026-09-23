cd /c/Source/HamLet
sh .run-unit/unit402-floors.sh floors-exit-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 4 of 5" "task 4: both lines done, app 278 counting dispatcher losses neither way, engine 178 in 371 s; captures floor running" --no-build
sh .run-unit/unit402-floors.sh floors-exit-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 4 of 5" "task 4: exit round, adjudicated floor running" --no-build
sh .run-unit/unit402-floors.sh floors-exit-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 4 of 5" "task 4: exit round, clean synthetics running" --no-build
sh .run-unit/unit402-six.sh exit "TASK 4 of 5" "task 4: exit round, floors done;" acq gate adj recv fixtures disp
echo "captures against entry:"; sh .run-unit/unit402-cmp.sh .run-unit/unit402-floors-1.txt .run-unit/unit402-floors-exit-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit402-adjcmp.sh .run-unit/unit402-adjud-entry.txt .run-unit/unit402-floors-exit-2.txt
for W in acq gate adj recv fixtures disp; do echo "--- $W against entry:"; diff .run-unit/unit402-$W-entry-list.txt .run-unit/unit402-$W-exit-list.txt; done
grep -hE "^ [0-9]+ wpm at|^ real signal|^ speeds named|^ [a-z-]+-easy: [0-9]+ characters|characters during" .run-unit/unit402-acq-exit.txt .run-unit/unit402-gate-exit.txt .run-unit/unit402-adj-exit.txt .run-unit/unit402-recv-exit.txt
sh .run-unit/unit402-transmit.sh
echo "src vs 162f0259:"; git diff --stat 162f0259 HEAD -- src; echo end
echo "tests docs vs 162f0259:"; git diff --stat 162f0259 HEAD -- tests docs; echo end
echo "status tests:"; git status --short -- tests; echo end
echo "worktrees:"; git worktree list
date +%H:%M:%S
