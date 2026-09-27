#!/bin/sh
# unit 467 - section 3 checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"; git log --oneline -1
echo "== plan copies differ?"; cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "identical"
echo "== step 9 lines"; grep -n -E "^\s*- \[[ x]\] \*?\*?9\.[0-9]" PHASE_PLAN.md | cut -c1-120
echo "== R86"; grep -n "Steps 2 to 8 are not authorable" PHASE_PLAN.md | cut -c1-160
echo "== port diff"; git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/ ; echo "(end port diff)"
echo "== switch table"; F=$(git ls-files src | grep CwSwitchTable); echo "$F"; grep -n -E "Live|Ours|Port|Arbitrate|TX-|char-gap|gap5|Farns|Itu" $F | cut -c1-160 | head -60
echo "== tests"; git ls-files tests | grep -E "TheArbitrationEarnsItsPlace"
echo "== status"; git status --short
echo "== version"; grep -n "<Version>" Directory.Build.props
