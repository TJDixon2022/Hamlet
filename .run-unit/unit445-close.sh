#!/bin/sh
# unit 445 - what src carries against entry, and the plan's step 3 lines, before closing.
cd /c/Source/HamLet || exit 1
echo "src against entry 0528afbe:"
git diff --stat 0528afbe -- src
echo "(end of src diff)"
git diff 0528afbe -- src | grep -iE "^\+.*(transmit|keyer|ptt|keydown|\bkey\(|sendcw|tx)" || echo "no added src line names transmit, keyer, PTT or TX"
git diff --stat 0528afbe -- tests | tail -3
grep -n "^- \[.\] 3\.[0-9]" PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md | cut -c1-70
git log --oneline -6
git status -sb | head -1
date "+%Y-%m-%d %H:%M"
