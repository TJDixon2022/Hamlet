#!/bin/sh
# unit 465 - task 0 commit: the runner's writes as they are, the record, the bump, the save printer, the entry printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "0 of 4" code none "Task 0 figures all as at 464s exit; saves of both decoders written (1467 ours lines, 818 port); committing"
git add -u -- .run-unit PHASE_OUTCOME.md PHASE_STATUS.md RUN_LEDGER.md WORK_INSTRUCTIONS.md docs/phase-requirements Directory.Build.props PROJECT_STATUS.md || exit 1
git add -- .run-unit/reports/unit-3-output-11.md tests/Hamlet.RadioEngine.Tests/Cw/WhereTheTwoReadingsMeetFact.cs || exit 1
for f in .run-unit/unit465-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git status --short | grep "^??"
git commit -q -F .run-unit/unit465-msg0.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
