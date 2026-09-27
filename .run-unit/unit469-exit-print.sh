#!/bin/sh
# unit 469 - the exit prints: src against e058da0c, the port against 19109b51, the eleven transmit files against 7e209cb4, git status, this unit's commits.
cd /c/Source/HamLet || exit 1
C=src/Hamlet.RadioEngine/Cw
echo "== git diff e058da0c -- src"
git diff e058da0c -- src
echo "== (end src diff)"
echo "== git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/"
git diff 19109b51 -- $C/Second/
echo "== (end port diff)"
echo "== git diff 7e209cb4 over the eleven transmit files"
git diff 7e209cb4 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs"
done
echo "== (end transmit diff)"
echo "== git status"
git status --short
echo "== this unit's commits"
git log --oneline e058da0c..HEAD
