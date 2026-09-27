#!/bin/sh
# unit 470 - task 0 commit: the runner's writes as they are, the record, the bump, the entry printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "0 of 3" code none "Task 0 entry figures all as at 469's exit, app line clean on the first run; committing the record and the runner's writes before the trace starts"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs/phase-requirements Directory.Build.props PROJECT_STATUS.md || exit 1
git add -- .run-unit/reports/unit-8-output-3.md || exit 1
for f in .run-unit/unit470-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
echo "== staged"
git status --short | grep -v "^??"
echo "== untracked"
git status --short | grep "^??"
git commit -q -F .run-unit/unit470-msg0.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
