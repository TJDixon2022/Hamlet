#!/bin/sh
# unit 413 - the eleven transmit files against 7e209cb4, and src/Hamlet.App against a given base.
# Usage: sh .run-unit/unit413-transmit.sh <base-for-app>
cd /c/Source/HamLet || exit 1
C=src/Hamlet.RadioEngine/Cw
echo "== transmit files against 7e209cb4"
git diff --stat 7e209cb4 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs"
done
echo "== end transmit"
echo "== src/Hamlet.App against $1"
git diff --stat "$1" -- src/Hamlet.App
echo "== end app"
