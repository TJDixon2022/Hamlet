#!/bin/sh
# unit 457 - the eleven transmit files (unit 435's list) against 7e209cb4, printed whole; and src this unit.
cd /c/Source/HamLet || exit 1
C=src/Hamlet.RadioEngine/Cw
echo "== git diff 7e209cb4 over the eleven transmit files:"
git diff 7e209cb4 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs"
done
echo "== end transmit"
echo "== src this unit against 19109b51, file by file:"
git diff --stat 19109b51 HEAD -- src
