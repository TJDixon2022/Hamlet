#!/bin/sh
# unit 478 task 3 - the diffs the exit round prints. Entry is 1b738a76, the commit this unit started on.
cd /c/Source/HamLet || exit 1
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "== (end transmit diff)"
echo "== git diff --stat 1b738a76 -- src/Hamlet.RadioEngine/Cw"
git diff --stat 1b738a76 -- src/Hamlet.RadioEngine/Cw
echo "== changed lines that are not doc-comment lines"
git diff -U0 1b738a76 -- src/Hamlet.RadioEngine/Cw | grep -E "^[-+] " | grep -v -E "^[-+] +///"
echo "== (end Cw diff)"
echo "== git diff --stat 1b738a76 -- src tests"
git diff --stat 1b738a76 -- src tests
echo "== this unit's commits"
git log --oneline 1b738a76..HEAD
echo "== any file under tests/fixtures touched this unit"
git diff --stat 1b738a76 -- tests/fixtures
echo "== (end fixtures)"
