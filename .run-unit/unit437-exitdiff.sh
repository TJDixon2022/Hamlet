#!/bin/sh
# unit 437 - the exit diffs: the eleven transmit files against 7e209cb4, the four named files against this unit's entry 3980e2a8, and all of src and data.
cd /c/Source/HamLet || exit 1
C=src/Hamlet.RadioEngine/Cw
echo "== transmit files against 7e209cb4"
git diff --stat 7e209cb4 -- $C/CwTransmitter.cs $C/KeyerCwSender.cs $C/TransmitChain.cs $C/AutoCall.cs $C/AutoCallAnswers.cs $C/CwTransmitGuard.cs $C/TransmissionWatch.cs $C/TransmitReadiness.cs $C/TransmitPrivileges.cs $C/TransmitNotes.cs $C/ICwSender.cs
for f in CwTransmitter KeyerCwSender TransmitChain AutoCall AutoCallAnswers CwTransmitGuard TransmissionWatch TransmitReadiness TransmitPrivileges TransmitNotes ICwSender
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs"
done
echo "== end transmit"
echo "== the four against entry 3980e2a8"
git diff --stat 3980e2a8 -- $C/CwToneTracker.cs $C/CwToneSurvey.cs $C/CwDecoder.cs $C/CwUnitEstimator.cs
for f in CwToneTracker CwToneSurvey CwDecoder CwUnitEstimator
do
  [ -f "$C/$f.cs" ] || echo "missing $C/$f.cs"
done
echo "== end four"
echo "== all of src and data against entry 3980e2a8"
git diff --stat 3980e2a8 -- src data
echo "== end src"
