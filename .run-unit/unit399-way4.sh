cd /c/Source/HamLet
# Way 4: one uncommitted hunk in CwProbabilisticDecoder.Estimate, measured, and put back in this script.
F=src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
sed -n 1057,1063p $F
sed -e "1056r .run-unit/unit399-way4-hunk.txt" -e "1057,1063d" $F > .run-unit/unit399-way4-patched.cs
cp .run-unit/unit399-way4-patched.cs $F
echo "the hunk as applied:"
git diff -- $F > .run-unit/unit399-way4-diff.txt
cat .run-unit/unit399-way4-diff.txt
sh .run-unit/unit399-build.sh way4 "task 1: way 4, uncommitted Estimate hunk applied, building"
sh .run-unit/unit399-floors.sh way4-3 300 "FullyQualifiedName~CwFixtureTests.TheCleanRecordingsDecodeExactly" "TASK 1 of 5" "task 1: way 4, synthetics under the hunk" --no-build
grep -h "^Actual" .run-unit/unit399-way4-3.txt
sh .run-unit/unit399-floors.sh way4-1 600 "FullyQualifiedName~TheCapturesThatDecodeKeepDecodingTests" "TASK 1 of 5" "task 1: way 4, captures type under the hunk" --no-build
sh .run-unit/unit399-floors.sh way4-2 600 "FullyQualifiedName~TheAdjudicatedReadingsKeepReadingTests" "TASK 1 of 5" "task 1: way 4, adjudicated type under the hunk" --no-build
echo "captures under the hunk against entry:"; sh .run-unit/unit399-cmp.sh .run-unit/unit399-floors-1.txt .run-unit/unit399-way4-1.txt
git checkout -- src/Hamlet.RadioEngine/Cw
echo "put back; src vs 5688a8a5:"; git diff --stat 5688a8a5 HEAD -- src; echo "end"
echo "working tree src:"; git status --short -- src; echo "end"
date +%H:%M:%S
