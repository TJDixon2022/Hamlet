#!/bin/sh
# unit 444 - what src carries against entry, and the plan's step 2 lines, before closing.
cd /c/Source/HamLet || exit 1
echo "src against entry 1f6a5789:"
git diff --stat 1f6a5789 -- src
echo "(end of src diff)"
git diff --stat 1f6a5789 -- tests | tail -3
grep -n "^- \[.\] 2\.[0-9]" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-90
git log --oneline -4
git status -sb | head -1
date "+%Y-%m-%d %H:%M"
