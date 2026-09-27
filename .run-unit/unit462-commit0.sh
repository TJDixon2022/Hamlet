#!/bin/sh
# unit 462 - task 0 commit: the runner's writes as they are, the record, the bump, the entry printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
cp .run-unit/unit462-v11-entry.txt .run-unit/unit462-v11-before.txt
sh tools/status.sh EXECUTING "0 of 4" code none "Task 0 figures all as at 461's exit; committing the record, the runner's writes and the entry printouts"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs/phase-requirements Directory.Build.props PROJECT_STATUS.md output.md || exit 1
git add -- .run-unit/reports/unit-8-output-2.md || exit 1
for f in .run-unit/unit462-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??" | head -70
git status --short | grep "^??"
git commit -q -F .run-unit/unit462-msg0.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
