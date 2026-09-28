#!/bin/sh
# unit 474 task 4 - the diffs the exit round prints. Entry is f8fc3921, the commit this unit started on.
cd /c/Source/HamLet || exit 1
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "== (end transmit diff)"
echo "== git diff f8fc3921 -- src/Hamlet.RadioEngine/Cw"
git diff f8fc3921 -- src/Hamlet.RadioEngine/Cw
echo "== (end Cw diff)"
echo "== git diff --stat f8fc3921 -- src tests"
git diff --stat f8fc3921 -- src tests
echo "== this unit's commits"
git log --oneline f8fc3921..HEAD
echo "== any file under tests/fixtures/cw touched this unit"
git diff --stat f8fc3921 -- tests/fixtures
echo "== (end fixtures)"
