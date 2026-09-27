#!/bin/sh
# unit 472 task 0 - section 3's checks against the tree.
cd /c/Source/HamLet || exit 1
echo "== clock"
date "+%Y-%m-%dT%H:%M:%S%:z"
echo "== HEAD"
git log --oneline -3
echo "== version"
grep -n "<Version>" Directory.Build.props
echo "== PHASE_PLAN step 2, 3 and 9 lines, root"
grep -n -E "^- \[.\] \*?\*?(2|3|9)\.[0-9]" PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN step 2, 3 and 9 lines, docs copy"
grep -n -E "^- \[.\] \*?\*?(2|3|9)\.[0-9]" docs/phase-requirements/PHASE_PLAN.md | cut -c1-120
echo "== PHASE_PLAN copies differ?"
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
echo "== R86 line"
grep -n "Steps 2 to 8 are not authorable until 9.4" PHASE_PLAN.md | cut -c1-200
echo "== 470 and 471 patches"
ls -l .run-unit/unit470-gate.patch .run-unit/unit471-reread.patch
echo "== port diff against 19109b51"
git diff --stat 19109b51 -- src/Hamlet.RadioEngine/Cw/Second/
echo "(end port diff)"
echo "== runner writes"
git status --short
echo "== watched.rc and watched.cpu"
ls -l .run-unit/watched.rc .run-unit/watched.cpu 2>&1
echo "== traffic net search: GRAY KC"
git grep -l -I "GRAY KC" | head -20
echo "== traffic net search: LIVER VIA"
git grep -l -I "LIVER VIA" | head -20
echo "== tracked files with 2026-09-25 to 27 in the name"
git ls-files | grep -E "2026-?09-?2[5-7]|20260925|20260926|20260927" | head -20
echo "== audio files by modification date since 2026-09-25"
find data tests assets scratch-audio -type f \( -iname "*.wav" -o -iname "*.flac" -o -iname "*.mp3" \) -newermt 2026-09-25 2>/dev/null | head -20
echo "(end search)"
