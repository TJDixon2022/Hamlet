#!/bin/sh
# unit 467 task 2 - the exit prints: the port, src against d34a0563, the eleven transmit files, git state and this unit's commits.
cd /c/Source/HamLet || exit 1
C=src/Hamlet.RadioEngine/Cw
echo "== git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/"
git diff 19109b51 -- $C/Second/
echo "== (end port diff)"
echo "== git diff d34a0563 -- src"
git diff d34a0563 -- src
echo "== (end src diff)"
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs"
done
echo "== (end transmit diff)"
echo "== git status"
git status --short
echo "== this unit's commits"
git log --oneline d34a0563..HEAD
