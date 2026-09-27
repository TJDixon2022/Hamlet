#!/bin/sh
# unit 465 task 0 - section 3's checks against the tree.
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
echo "== 464 work present"
grep -n "Probability" src/Hamlet.RadioEngine/Cw/CwCharacter.cs | head -5
ls src/Hamlet.RadioEngine/Cw/FldigiConfidence.cs
grep -rln "class CwCalibration" tests | head
grep -n -i -E "calibrated|verdict" docs/phase-requirements/calibration.md | head -40
