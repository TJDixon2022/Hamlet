#!/bin/sh
# unit 461 - task 0 commit: the runner's writes as they are, the record, the bump, the entry printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs/phase-requirements Directory.Build.props PROJECT_STATUS.md || exit 1
git add -- .run-unit/reports/unit-7-output-3.md || exit 1
for f in .run-unit/unit461-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??" | head -60
git status --short | grep "^??"
git commit -q -F .run-unit/unit461-msg0.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
