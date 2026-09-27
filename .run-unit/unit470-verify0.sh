#!/bin/sh
# unit 470 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"
git log --oneline -1
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== PHASE_PLAN step 2, 7 and 9 lines, root"
grep -n -E "^- \[.\] \*?\*?(2|7|9)\.[0-9]" PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN step 2, 7 and 9 lines, docs copy"
grep -n -E "^- \[.\] \*?\*?(2|7|9)\.[0-9]" docs/phase-requirements/PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN copies differ?"
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
echo "== R86 line"
grep -n "Steps 2 to 8 are not authorable until 9.4" PHASE_PLAN.md | cut -c1-160
echo "== PARKED RESOLVED lines"
grep -n "RESOLVED:" PARKED.md | cut -c1-600
echo "== proof states in src"
grep -rn -E "enum CwPitchProof|enum CwSpeedProof|class CwPitchProof|class CwSpeedProof|record CwPitchProof|record CwSpeedProof" src
grep -rln -E "CwPitchProof|CwSpeedProof" src tests
echo "== port diff against 19109b51"
git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end port diff)"
echo "== runner writes"
git status --short
