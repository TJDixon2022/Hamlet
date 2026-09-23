#!/bin/sh
# unit 404 - section 5 checks and the transmit-file diff against 7e209cb4
cd /c/Source/HamLet || exit 1
echo "== commits after bc2484d5"
git log --oneline bc2484d5..HEAD
echo "== newest src commit"
git log -1 --format=%h -- src
echo "== printer"
ls tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs
grep -n "WEEKEND\|THINKING\|FLEX\|ABOVE\|BREEZE" tests/Hamlet.RadioEngine.Tests/Cw/TheReworkNumbersPrinterTests.cs | head -10
echo "== transmit files"
TX=""
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender; do
  p=$(git ls-files "src/*/$f.cs" "src/*/*/$f.cs" "src/*/*/*/$f.cs")
  echo "$f -> $p"
  TX="$TX $p"
done
echo "$TX" > .run-unit/unit404-txfiles.txt
echo "== git diff 7e209cb4 HEAD over transmit files"
git diff --stat 7e209cb4 HEAD -- $TX
echo "== end transmit diff"
echo "== src/Hamlet.App diff vs HEAD (working tree)"
git diff --stat HEAD -- src/Hamlet.App
echo "== end"
