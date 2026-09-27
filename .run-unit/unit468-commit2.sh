#!/bin/sh
# unit 468 - task 2 commit: the fact, senders.md, metrics.md, the printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "2 of 3" code none "Task 2 done - HM-REQ-050 not met on TX-ITU, KEYER-W, FARNS, TIGHT; floors and both lines green; committing, then the 7.2 tick"
git add -- tests/Hamlet.RadioEngine.Tests/Cw/TheMustFistsAtFifteenDecibelsFact.cs docs/phase-requirements/senders.md docs/phase-requirements/metrics.md PROJECT_STATUS.md || exit 1
for f in .run-unit/unit468-*
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
git commit -q -F .run-unit/unit468-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
