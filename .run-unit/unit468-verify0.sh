#!/bin/sh
# unit 468 - section 3 checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== HEAD"; git log --oneline -1
echo "== plan copies differ?"; cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "identical"
echo "== step 7 lines"; grep -n -E "^\s*- \[[ x]\] \*?\*?7\.[0-9]" PHASE_PLAN.md | cut -c1-160
echo "== step 9 lines"; grep -n -E "^\s*- \[[ x]\] \*?\*?9\.[0-9]" PHASE_PLAN.md | cut -c1-120
echo "== R86"; grep -n "Steps 2 to 8 are not authorable" PHASE_PLAN.md | cut -c1-160
echo "== port diff"; git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/ ; echo "(end port diff)"
echo "== channel layer"; git ls-files | grep -E "CwChannel|TheChannelProfilesAreWhatTheySay|channels.md|SyntheticCq|CwFixtureGenerator|CwFixtureRecipe"
grep -n "CH-AWGN" $(git ls-files | grep "CwChannel.cs") | head -5
echo "== status"; git status --short
echo "== version"; grep -n "<Version>" Directory.Build.props
