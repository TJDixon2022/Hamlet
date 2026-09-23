cd /c/Source/HamLet
echo "callers reading the clean files or CwFixtures:"
grep -ln "clean-12wpm\|clean-18wpm\|CwFixtures\." tests/Hamlet.RadioEngine.Tests/Cw/CwAcquisitionWindowTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwSensitivityTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwSensitivity.cs tests/Hamlet.RadioEngine.Tests/Cw/EveryCharacterCarriesItsOwnEvidenceTests.cs tests/Hamlet.RadioEngine.Tests/Cw/WhereAcquisitionPointsTests.cs tests/Hamlet.RadioEngine.Tests/Cw/CwRefusalFloorTableTests.cs; echo end
echo "all readers of clean-12wpm / clean-18wpm / CwFixtures.All under tests:"; grep -rln "clean-12wpm\|clean-18wpm\|CwFixtures\.All" tests --include=*.cs; echo end
sh .run-unit/unit400-floors.sh floors-band-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 2 of 5" "task 2: clean fixtures regenerated at 0.02, 2 of 2 exact; captures type running" --no-build
sh .run-unit/unit400-floors.sh floors-band-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 2 of 5" "task 2: adjudicated type running on the regenerated fixtures" --no-build
echo "captures against entry:"; sh .run-unit/unit400-cmp.sh .run-unit/unit400-floors-1.txt .run-unit/unit400-floors-band-1.txt
echo "adjudicated against entry:"; sh .run-unit/unit400-adjcmp.sh .run-unit/unit400-floors-2.txt .run-unit/unit400-floors-band-2.txt
sh .run-unit/unit400-floors.sh cleanreads-band 300 "FullyQualifiedName~TheCleanReadsStayCleanTests" "TASK 2 of 5" "task 2: TheCleanReadsStayCleanTests running" --no-build
sh .run-unit/unit400-floors.sh survey-band 300 "FullyQualifiedName~TheSurveyAlreadyUsesAShortWindowTests" "TASK 2 of 5" "task 2: TheSurveyAlreadyUsesAShortWindowTests running" --no-build
grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-cleanreads-after.txt .run-unit/unit400-survey-after.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-cr-a.txt
grep -hE "^\s+(Passed|Failed) " .run-unit/unit400-cleanreads-band.txt .run-unit/unit400-survey-band.txt | sed -E "s/ \[.*//" | sed -E "s/^\s+//" | sort > .run-unit/unit400-cr-b.txt
echo "clean reads and survey task 1 against task 2:"; diff .run-unit/unit400-cr-a.txt .run-unit/unit400-cr-b.txt; echo "diff rc $?"
ls -l tests/fixtures/cw/clean-12wpm.wav tests/fixtures/cw/clean-18wpm.wav
git diff --stat -- tests
date +%H:%M:%S
