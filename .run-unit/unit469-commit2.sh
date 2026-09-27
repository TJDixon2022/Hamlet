#!/bin/sh
# unit 469 - task 2 commit: the fact, interference.md, metrics.md, the printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "2 of 3" code none "Task 2 - 060 2 of 6, 061 0 of 6, 062 4 of 12 not met; 066 3 of 3 weak; round green (app on the rerun, lost type alone 3/3); committing"
git add -- tests/Hamlet.RadioEngine.Tests/Cw/TheInterferenceIsMeasuredFact.cs docs/phase-requirements/interference.md docs/phase-requirements/metrics.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit469-*
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
git commit -q -F .run-unit/unit469-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
