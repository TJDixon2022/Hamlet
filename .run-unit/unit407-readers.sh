#!/bin/sh
# unit 407 - list the tracker readers: every test file naming CwToneTracker, its classes, facts and skips.
cd /c/Source/HamLet || exit 1
grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj | head -30
for f in $(grep -rl "CwToneTracker" tests/Hamlet.RadioEngine.Tests --include=*.cs) tests/Hamlet.RadioEngine.Tests/Cw/ThePitchCanBeHeldTests.cs
do
  C=$(grep -hoE "class [A-Za-z0-9_]+" "$f" | tr '\n' ' ')
  N=$(grep -cE "\[(Fact|Theory)" "$f")
  S=$(grep -c "Skip" "$f")
  echo "$f | $C| facts $N | skip $S"
done
