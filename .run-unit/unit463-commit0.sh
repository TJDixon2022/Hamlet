#!/bin/sh
# unit 463 - task 0 commit: the runner's writes as they are, the record, the bump, the entry printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
cp .run-unit/unit463-v11-entry.txt .run-unit/unit463-v11-before.txt
sh tools/status.sh EXECUTING "0 of 4" code none "Task 0 figures all as at 462s exit; committing the record, the runner writes and the entry printouts"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs/phase-requirements Directory.Build.props PROJECT_STATUS.md || exit 1
git add -- .run-unit/reports/unit-1-output-23.md || exit 1
for f in .run-unit/unit463-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git status --short | grep "^??"
git commit -q -F .run-unit/unit463-msg0.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
