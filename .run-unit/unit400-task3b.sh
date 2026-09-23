cd /c/Source/HamLet
sh .run-unit/unit400-build.sh prosigns-w "task 3: prosigns request carries 0.02, building with the temporary writer fact"
grep -q " 0 Error(s)" .run-unit/unit400-build-prosigns-w.txt || { echo "STOP: writer build failed"; exit 1; }
sh .run-unit/unit400-floors.sh writer-prosigns 300 "FullyQualifiedName~Unit400WriteTheProsignsFixture" "TASK 3 of 5" "task 3: writer fact regenerating prosigns-18wpm at 0.02" --no-build
grep -q "Passed: *1" .run-unit/unit400-writer-prosigns.txt || { echo "STOP: writer did not pass"; exit 1; }
echo "fixtures status:"; git status --short tests/fixtures/cw; echo end
rm -f tests/Hamlet.RadioEngine.Tests/Cw/Unit400WriteTheProsignsFixture.cs
echo "tests status:"; git status --short tests; echo end
sh .run-unit/unit400-build.sh prosigns-r "task 3: writer deleted, rebuilding before judging the regenerated prosigns fixture"
grep -q " 0 Error(s)" .run-unit/unit400-build-prosigns-r.txt || { echo "STOP: rebuild failed"; exit 1; }
sh .run-unit/unit400-floors.sh fixtures-prosigns 600 "FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests" "TASK 3 of 5" "task 3: CwFixtureTests whole on the regenerated prosigns fixture running" --no-build
grep -E "^\s+(Passed|Failed) " .run-unit/unit400-fixtures-prosigns.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort
sh .run-unit/unit400-floors.sh floors-pro-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 3 of 5" "task 3: prosigns regenerated; captures type running" --no-build
sh .run-unit/unit400-floors.sh floors-pro-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 3 of 5" "task 3: prosigns regenerated; adjudicated type running" --no-build
echo "captures against entry:"; sh .run-unit/unit400-cmp.sh .run-unit/unit400-floors-1.txt .run-unit/unit400-floors-pro-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit400-adjcmp.sh .run-unit/unit400-floors-2.txt .run-unit/unit400-floors-pro-2.txt
sh .run-unit/unit400-floors.sh cleanreads-pro 300 "FullyQualifiedName~TheCleanReadsStayCleanTests" "TASK 3 of 5" "task 3: TheCleanReadsStayCleanTests running" --no-build
sh .run-unit/unit400-floors.sh survey-pro 300 "FullyQualifiedName~TheSurveyAlreadyUsesAShortWindowTests" "TASK 3 of 5" "task 3: TheSurveyAlreadyUsesAShortWindowTests running" --no-build
grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-cleanreads-pro.txt .run-unit/unit400-survey-pro.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-cr-c.txt
echo "clean reads and survey task 2 against task 3:"; diff .run-unit/unit400-cr-b.txt .run-unit/unit400-cr-c.txt; echo "diff rc $?"
date +%H:%M:%S
