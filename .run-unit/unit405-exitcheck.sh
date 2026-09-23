cd /c/Source/HamLet
echo "== the six reds' numbers at exit"
grep -hE "^\s+(12|25) wpm at 18 dB, (run-up|bare)" .run-unit/unit405-acq-exit.txt | sort -u
grep -E "came back with" .run-unit/unit405-recv-exit.txt | grep -v xUnit | cut -c1-120
grep -E "characters during the preamble" .run-unit/unit405-recv-exit.txt | grep -v xUnit
echo "== transmit files vs 7e209cb4"
T=""
for f in CwTransmitter.cs KeyerCwSender.cs TransmitChain.cs AutoCall.cs AutoCallAnswers.cs CwTransmitGuard.cs TransmissionWatch.cs TransmitReadiness.cs TransmitPrivileges.cs TransmitNotes.cs ICwSender.cs
do
  T="$T $(git ls-files "src/*/$f")"
done
git diff --stat 7e209cb4 HEAD -- $T
git diff --stat HEAD -- $T
echo "== src entry to exit"
git diff --stat f74b6d51 HEAD -- src
git diff --stat HEAD -- src
echo "== src/Hamlet.App entry to exit"
git diff --stat f74b6d51 HEAD -- src/Hamlet.App
echo "== tests entry to exit"
git diff --stat f74b6d51 HEAD -- tests
git status --short -- src tests
echo "== 3.5 in PHASE_PLAN.md"
grep -n "3\.5" docs/phase-cw/PHASE_PLAN.md PHASE_PLAN.md 2>/dev/null | head -20
echo "== end"
