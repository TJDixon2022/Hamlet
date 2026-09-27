#!/bin/sh
# unit 465 - task 1 commit: the fact and its printout, this unit's run files; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "1 of 4" code none "Task 1 - rules fixed (half-shorter-span overlap, 464s held-out verdicts, margin 0.05); counts in; committing the fact"
git add -- PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/WhereTheTwoReadingsMeetFact.cs || exit 1
for f in .run-unit/unit465-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit465-msg1.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
