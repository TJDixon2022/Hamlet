#!/bin/sh
# unit 465 - tick 9.6 in both copies of PHASE_PLAN.md and nothing else; commit with the watch evidence; push.
cd /c/Source/HamLet || exit 1
for f in PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
do
  sed -i 's/^- \[ \] 9\.6 HM-REQ-120, 121, 125, 126, 127:/- [x] 9.6 HM-REQ-120, 121, 125, 126, 127:/' "$f"
  grep -n -E "^- \[.\] 9\.[0-9]" "$f" | cut -c1-40
done
cmp PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md && echo "copies identical"
git diff --stat -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md
sh tools/status.sh EXECUTING "3 of 4" code none "Task 3 - all six of 9.6s conditions hold, each test green and watched red; ticking 9.6 in both plan copies"
git add -- PHASE_PLAN.md docs/phase-requirements/PHASE_PLAN.md PROJECT_STATUS.md tests/Hamlet.App.Tests/ViewModels/TheOperatorSeesOneTranscriptTests.cs || exit 1
for f in .run-unit/unit465-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit465-msg-tick.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
