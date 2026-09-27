#!/bin/sh
# unit 471 - refused: keep the re-read and its test as a patch under .run-unit, then restore src and tests to HEAD.
cd /c/Source/HamLet || exit 1
T=tests/Hamlet.RadioEngine.Tests/Cw/TheAcquiringStretchIsReadAtTheProvedValuesTests.cs
P=.run-unit/unit471-reread.patch
git add -N -- "$T" || exit 1
git diff -- src tests > "$P"
git reset -q -- "$T"
echo "== patch"
grep -E "^(diff|\+\+\+|---) " "$P"
wc -l "$P"
mv "$T" .run-unit/unit471-TheAcquiringStretchIsReadAtTheProvedValuesTests.cs.txt || exit 1
git checkout -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs src/Hamlet.RadioEngine/Cw/CwProbabilisticStream.cs || exit 1
echo "== src and tests against HEAD after the restore"
git status --short -- src tests
git diff --stat -- src tests
echo "(end)"
