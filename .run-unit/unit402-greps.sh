cd /c/Source/HamLet
echo "=== failure messages"
for W in acq gate adj recv fixtures; do
  echo "--- $W"
  grep -E "Assert|Expected|Actual|share|Share|characters|Error Message" -A0 .run-unit/unit402-$W-entry.txt | grep -v "^\s*at " | cut -c1-300 | head -40
done
echo "=== grep speed"
grep -n "WordsPerMinute\|SpeedIsReacquiring" tests/Hamlet.RadioEngine.Tests/Cw/*.cs tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/*.cs | cut -c1-200
echo "=== compile removes"
grep -n "Compile Remove" tests/Hamlet.RadioEngine.Tests/Hamlet.RadioEngine.Tests.csproj
echo "=== NoSingleClock"
grep -rn "NoSingleClockIsFittedAcrossBothStations" tests
echo "=== src greps"
grep -n "OwnTransmitSeconds\|_samplesAtDiscontinuity\|_hasFollowed" src/Hamlet.RadioEngine/Cw/*.cs
echo "=== InternalsVisibleTo"
grep -rn "InternalsVisibleTo" src/Hamlet.RadioEngine --include=*.cs --include=*.csproj | head
echo "=== captures rows"
sh .run-unit/unit402-nums.sh .run-unit/unit402-floors-1.txt
echo "=== captures vs 401 exit"
sh .run-unit/unit402-cmp.sh .run-unit/unit401-floors-exit-1.txt .run-unit/unit402-floors-1.txt
