#!/bin/sh
# unit 470 - refused: keep the gate and its test as a patch under .run-unit, then restore src and tests to HEAD.
cd /c/Source/HamLet || exit 1
T=tests/Hamlet.RadioEngine.Tests/Cw/NoSureLetterWhileAcquiringTests.cs
P=.run-unit/unit470-gate.patch
git add -N -- "$T" || exit 1
git diff -- src tests > "$P"
git reset -q -- "$T"
echo "== patch"
grep -E "^(diff|\+\+\+|---) " "$P"
wc -l "$P"
mv "$T" .run-unit/unit470-NoSureLetterWhileAcquiringTests.cs.txt || exit 1
git checkout -- src/Hamlet.RadioEngine/Cw/CwDecoder.cs || exit 1
echo "== src and tests against HEAD after the restore"
git status --short -- src tests
git diff --stat -- src tests
echo "(end)"
