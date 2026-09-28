#!/bin/sh
# unit 480 (letter over the bars) task 3 - the diffs the exit round prints. Entry is a06e08c4.
cd /c/Source/HamLet || exit 1
echo "== git diff --stat a06e08c4 -- src/Hamlet.RadioEngine"
git diff --stat a06e08c4 -- src/Hamlet.RadioEngine
echo "== (end engine diff)"
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "== (end transmit diff)"
ls src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs | wc -l
echo "== git diff --stat a06e08c4 -- src tests"
git diff --stat a06e08c4 -- src tests
echo "== tests/fixtures"
git diff --stat a06e08c4 -- tests/fixtures
echo "== this unit's commits"
git log --oneline a06e08c4..HEAD | cut -c1-90
