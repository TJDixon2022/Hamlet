#!/bin/sh
# unit 466 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"
git log --oneline -1
echo "== PHASE_PLAN step 9 lines, root"
grep -n -E "^- \[.\] \*?\*?9\.[0-9]" PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN copies differ?"
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
echo "== R86 line"
grep -n "Steps 2 to 8 are not authorable until 9.4" PHASE_PLAN.md | cut -c1-160
echo "== port diff against 19109b51"
git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end port diff)"
echo "== 465 work present"
grep -rn -E "class CwArbiter|class CwVoteTable|TieMargin *=|class CwSecondReader" src | cut -c1-200
grep -rln -E "class (TheHigherCalibratedReadingWinsTests|BothDecodersReadTheSameSamplesTests|TheOperatorSeesOneTranscriptTests|WhereTheTwoReadingsMeetFact)" tests
grep -rn -i "\"arbiter" src | head -5
echo "== runner writes"
git status --short
echo "== version"
grep -n "<Version>" Directory.Build.props
