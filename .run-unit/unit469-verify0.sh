#!/bin/sh
# unit 469 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"
git log --oneline -1
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== PHASE_PLAN step 7 and 9 lines, root"
grep -n -E "^- \[.\] \*?\*?(7|9)\.[0-9]" PHASE_PLAN.md | cut -c1-140
echo "== PHASE_PLAN step 7 and 9 lines, docs copy"
grep -n -E "^- \[.\] \*?\*?(7|9)\.[0-9]" docs/phase-requirements/PHASE_PLAN.md | cut -c1-140
echo "== PHASE_PLAN copies differ?"
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
echo "== R86 line"
grep -n "Steps 2 to 8 are not authorable until 9.4" PHASE_PLAN.md | cut -c1-160
echo "== port diff against 19109b51"
git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end port diff)"
echo "== 461 and 468 layers"
grep -rln -E "class CwChannel|class CwSender" tests src
grep -rn -E "\"CH-AWGN\"|\"TX-ITU\"" tests | cut -c1-160 | head -6
ls docs/phase-requirements/channels.md docs/phase-requirements/senders.md
echo "== two-signal fixtures"
ls tests/Hamlet.RadioEngine.Tests/Cw/Fixtures/ | grep -i -E "two|station|channel|sender|interf"
echo "== runner writes"
git status --short
