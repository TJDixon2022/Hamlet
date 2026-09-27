#!/bin/sh
# unit 471 task 4 - the diffs and the state the exit round prints.
cd /c/Source/HamLet || exit 1
echo "== git diff a2e07f6d -- src"
git diff a2e07f6d -- src
echo "== (end src diff)"
echo "== git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/"
git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "== (end port diff)"
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "== (end transmit diff)"
ls src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs | wc -l
echo "== git diff a2e07f6d -- tests (the fact only)"
git diff --stat a2e07f6d -- tests
echo "== git status"
git status -sb
echo "== this unit's commits"
git log --oneline a2e07f6d..HEAD
