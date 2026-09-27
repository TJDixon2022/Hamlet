#!/bin/sh
# unit 467 - task 0 commit: the runner's writes as they are, the record, the bump, the entry printouts and saves; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
git status --short
sh tools/status.sh EXECUTING "0 of 2" code none "Task 0 entry figures all as at 466's exit (decode time 65.50 s against 64.27 s); both emitted saves byte-identical to 466's exit; committing"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs/phase-requirements Directory.Build.props PROJECT_STATUS.md || exit 1
git add -- .run-unit/reports/unit-5-output-5.md || exit 1
for f in .run-unit/unit467-*
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
git commit -q -F .run-unit/unit467-msg0.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
