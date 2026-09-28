#!/bin/sh
# unit 480 task 4 - the diffs the exit round prints. Entry is 8226ccc4, the commit this unit started on.
cd /c/Source/HamLet || exit 1
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "== (end transmit diff)"
echo "== git diff --stat 8226ccc4 -- src/Hamlet.RadioEngine"
git diff --stat 8226ccc4 -- src/Hamlet.RadioEngine
echo "== git diff --stat 8226ccc4 -- src tests data"
git diff --stat 8226ccc4 -- src tests data
echo "== this unit's commits"
git log --oneline 8226ccc4..HEAD
echo "== any file under tests/fixtures touched this unit"
git diff --stat 8226ccc4 -- tests/fixtures
echo "== (end fixtures)"
