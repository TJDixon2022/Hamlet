#!/bin/sh
# unit 471 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== clock"
date "+%Y-%m-%dT%H:%M:%S%:z"
echo "== HEAD"
git log --oneline -3
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== PHASE_PLAN step 2 and 9 lines, root"
grep -n -E "^- \[.\] \*?\*?(2|9)\.[0-9]" PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN step 2 and 9 lines, docs copy"
grep -n -E "^- \[.\] \*?\*?(2|9)\.[0-9]" docs/phase-requirements/PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN copies differ?"
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
echo "== R86 line"
grep -n "Steps 2 to 8 are not authorable until 9.4" PHASE_PLAN.md | cut -c1-200
echo "== PARKED header"
head -12 PARKED.md
echo "== PARKED never-by-a-session"
grep -n "never by a session" PARKED.md | cut -c1-200
echo "== PARKED RESOLVED lines"
grep -n "RESOLVED:" PARKED.md | cut -c1-300
echo "== PARKED PARKED: lines"
grep -n "^PARKED:" PARKED.md | cut -c1-300
echo "== 470 patch"
ls -l .run-unit/unit470-gate.patch
echo "== port diff against 19109b51"
git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end port diff)"
echo "== runner writes"
git status --short
echo "== watched.rc"
ls -l .run-unit/watched.rc .run-unit/watched.cpu 2>&1
echo "== launch time in PHASE_OUTCOME"
grep -n -i -E "launch" PHASE_OUTCOME.md | tail -8 | cut -c1-200
