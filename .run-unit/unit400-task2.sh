cd /c/Source/HamLet
# Task 2: band 0.02 on the two clean requests, write the two files, delete the writer, judge.
# Stops if the writer build fails or the writer does not pass.
sh .run-unit/unit400-build.sh band "task 2: clean requests carry NoiseAmplitude 0.02, building with the temporary writer fact, second attempt after a missing using"
grep -q " 0 Error(s)" .run-unit/unit400-build-band.txt || { echo "STOP: writer build failed"; exit 1; }
sh .run-unit/unit400-floors.sh writer 300 "FullyQualifiedName~Unit400WriteTheCleanFixtures" "TASK 2 of 5" "task 2: writer fact regenerating clean-12wpm and clean-18wpm at 0.02" --no-build
grep -q "Passed: *1" .run-unit/unit400-writer.txt || { echo "STOP: writer did not pass"; exit 1; }
echo "fixtures status:"; git status --short tests/fixtures/cw; echo end
git add tests/Hamlet.RadioEngine.Tests/Cw/Unit400WriteTheCleanFixtures.cs
git rm -q -f tests/Hamlet.RadioEngine.Tests/Cw/Unit400WriteTheCleanFixtures.cs
echo "tests status:"; git status --short tests; echo end
sh .run-unit/unit400-build.sh band2 "task 2: writer deleted, rebuilding before judging the regenerated fixtures"
grep -q " 0 Error(s)" .run-unit/unit400-build-band2.txt || { echo "STOP: rebuild failed"; exit 1; }
sh .run-unit/unit400-floors.sh fixtures-band 600 "FullyQualifiedName~Hamlet.RadioEngine.Tests.Cw.CwFixtureTests" "TASK 2 of 5" "task 2: CwFixtureTests whole on the regenerated clean fixtures running" --no-build
grep -E "^\s+(Passed|Failed) " .run-unit/unit400-fixtures-band.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | sed -E "s/ \[.*//" | sort
grep -hE "^Actual|^Expected| gave back " .run-unit/unit400-fixtures-band.txt | head -12 | cut -c1-240
date +%H:%M:%S
