#!/bin/sh
# unit 463 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"
git log --oneline -1
echo "== PHASE_PLAN step 9 lines, root then docs copy"
grep -n -E "^- \[.\] \*?\*?9\.[0-9]" PHASE_PLAN.md
grep -n -E "^- \[.\] \*?\*?9\.[0-9]" docs/phase-requirements/PHASE_PLAN.md
echo "== R86 in both"
grep -n "Steps 2 to 8 are not authorable" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-160
echo "== plan copies differ?"
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "PHASE_PLAN copies identical"
echo "== the port against 19109b51 (empty)"
git diff 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "== end port"
ls src/Hamlet.RadioEngine/Cw/Second/ | head -30
echo "== IntegratorBandwidthHz and IntegratorWindow uses"
grep -rn "IntegratorBandwidthHz\|IntegratorWindow" src/Hamlet.RadioEngine/Cw --include=*.cs | grep -v "/Second/" | cut -c1-220
echo "== unit 462 saves"
ls -la .run-unit/unit462-B-refused.patch .run-unit/unit462-C-refused.patch .run-unit/unit462-W-refused.patch .run-unit/unit462-B-refused.txt .run-unit/unit462-C-refused.txt .run-unit/unit462-W-refused.txt .run-unit/unit462-text-before-classes.txt
echo "== watched.rc"
ls -la .run-unit/watched.rc .run-unit/watched.cpu 2>&1
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== PHASE_STATUS copies"
diff PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
echo "== DEC_RATIO"
grep -n "DEC_RATIO" .run-unit/fldigi/src/include/cw.h
cd .run-unit/fldigi && git log --oneline -1 2>&1 | head -1
