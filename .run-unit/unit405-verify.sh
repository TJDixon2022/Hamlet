cd /c/Source/HamLet
echo "== HEAD"
git rev-parse --short HEAD
echo "== f74b6d51 ancestor?"
git merge-base --is-ancestor f74b6d51 HEAD && echo yes
echo "== diff 7e65aac4 HEAD src"
git diff --stat 7e65aac4 HEAD -- src
echo "== diff f74b6d51 HEAD src tests list"
git diff --stat f74b6d51 HEAD -- src tests docs/carry-forward-tests.txt
F=src/Hamlet.RadioEngine/Cw/CwDecoder.cs
echo "== CwDecoder lines"
wc -l < $F
sed -n "513p
590p
701p" $F
P=src/Hamlet.RadioEngine/Cw/CwProbabilisticDecoder.cs
echo "== Prob 438 459"
sed -n "438p
459p" $P
echo "== fixture 203"
sed -n "203p" tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/CwReceiverFixtureTests.cs
echo "== EightReds"
ls tests/Hamlet.RadioEngine.Tests/Cw/TheEightRedsTests.cs
grep -c "Assert" tests/Hamlet.RadioEngine.Tests/Cw/TheEightRedsTests.cs
grep -n "EightReds\|CaptureOfTheTwentyThird" docs/carry-forward-tests.txt
echo "== failing set last line"
tail -n 1 docs/unit239-failing-set.txt
echo "== reds-3.6 rows"
grep -n "^|" docs/phase-cw/reds-3.6.md
echo "== transmit files vs 7e209cb4"
T=""
for f in CwTransmitter.cs KeyerCwSender.cs TransmitChain.cs AutoCall.cs AutoCallAnswers.cs CwTransmitGuard.cs TransmissionWatch.cs TransmitReadiness.cs TransmitPrivileges.cs TransmitNotes.cs ICwSender.cs
do
  p=$(git ls-files "src/*/$f")
  if [ -z "$p" ]
  then
    echo "missing $f"
  fi
  T="$T $p"
done
echo "paths:$T"
git diff --stat 7e209cb4 HEAD -- $T
git diff --stat HEAD -- $T
echo "== end"
