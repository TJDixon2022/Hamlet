cd /c/Source/HamLet
# Task 3: break CwDecoder.Process, run the guard alone red, put the file back, run it again green.
F="FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests|(FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests&DisplayName~cw-2026-08-25)"
P=tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
D=src/Hamlet.RadioEngine/Cw/CwDecoder.cs

echo "== step 1: break"
sed -n 499p $D
sed -i '499a\        if (chunk.Samples.Length >= 0) return; // UNIT 393 TASK 3 BREAK - NEVER COMMITTED' $D
git diff HEAD -- $D
git diff --stat HEAD -- src
git status --short src

echo "== step 2: guard alone against the broken decoder"
sh tools/status.sh EXECUTING "TASK 3 of 5" code none "guard alone against a CwDecoder that hands nothing on: 26 cases running, expected red on the assertion, timeout 300"
S=$(date +%s)
timeout 300 dotnet test $P --filter "$F" --logger "console;verbosity=detailed" > .run-unit/unit393-guard-red.txt 2>&1
echo "red run exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit393-guard-red.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | cut -c1-200
grep -E "Test Run Aborted|active test run was aborted" .run-unit/unit393-guard-red.txt | head -3
tail -4 .run-unit/unit393-guard-red.txt
git status --short src

echo "== step 3: put the file back"
git checkout HEAD -- $D
git status --short src
echo "== src clean if nothing above"

echo "== step 4: guard alone again"
sh tools/status.sh EXECUTING "TASK 3 of 5" code none "guard red run done, CwDecoder.cs back to HEAD; guard alone again, expected green, timeout 300"
S=$(date +%s)
timeout 300 dotnet test $P --filter "$F" --logger "console;verbosity=detailed" > .run-unit/unit393-guard-green.txt 2>&1
echo "green run exit $? in $(( $(date +%s) - S )) s"
grep -E "^\s+(Passed|Failed) " .run-unit/unit393-guard-green.txt | sed -E "s/^\s+//" | sed -E "s/Hamlet.RadioEngine.Tests.Cw.//" | cut -c1-200
tail -4 .run-unit/unit393-guard-green.txt
git status --short src
git diff --stat HEAD -- src
echo "== end"
