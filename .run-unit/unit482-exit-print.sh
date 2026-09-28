#!/bin/sh
# unit 482 task 3 - the diffs the exit round prints. Entry is 56fdea84, the commit this unit started on.
cd /c/Source/HamLet || exit 1
echo "== git diff 7e209cb4 over the eleven transmit files PARKED.md names"
git diff 7e209cb4 -- src/Hamlet.RadioEngine/Cw/CwTransmitter.cs src/Hamlet.RadioEngine/Cw/KeyerCwSender.cs src/Hamlet.RadioEngine/Cw/TransmitChain.cs src/Hamlet.RadioEngine/Cw/AutoCall.cs src/Hamlet.RadioEngine/Cw/AutoCallAnswers.cs src/Hamlet.RadioEngine/Cw/CwTransmitGuard.cs src/Hamlet.RadioEngine/Cw/TransmissionWatch.cs src/Hamlet.RadioEngine/Cw/TransmitReadiness.cs src/Hamlet.RadioEngine/Cw/TransmitPrivileges.cs src/Hamlet.RadioEngine/Cw/TransmitNotes.cs src/Hamlet.RadioEngine/Cw/ICwSender.cs
echo "== (end transmit diff)"
echo "== git diff --stat 56fdea84 -- src/Hamlet.RadioEngine"
git diff --stat 56fdea84 -- src/Hamlet.RadioEngine
echo "== (end engine diff)"
echo "== git diff --stat 56fdea84 -- src tests data"
git diff --stat 56fdea84 -- src tests data
echo "== this unit's commits"
git log --oneline 56fdea84..HEAD | cut -c1-120
echo "== any file under tests/fixtures touched this unit"
git diff --stat 56fdea84 -- tests/fixtures
echo "== (end fixtures)"
echo "== the 11.x lines in both plans"
grep -n "^- \[.\] 11\." PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-300
