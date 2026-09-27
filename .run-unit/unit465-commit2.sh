#!/bin/sh
# unit 465 - task 2 commit: the arbiter, the vote table, the margin, the harvester, the test, the fact's corpus check; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "2 of 4" code none "Task 2 - arbiter in, 8 of 8 green after 8 of 8 red; corpus identical to ours on all 35; committing"
C=src/Hamlet.RadioEngine/Cw
git add -- PROJECT_STATUS.md $C/CwArbiter.cs $C/CwVoteTable.cs $C/CwSecondReading.cs $C/CwCharacter.cs tests/Hamlet.RadioEngine.Tests/Cw/TheHigherCalibratedReadingWinsTests.cs tests/Hamlet.RadioEngine.Tests/Cw/WhereTheTwoReadingsMeetFact.cs || exit 1
for f in .run-unit/unit465-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git status --short -- src tests | grep "^??"
git commit -q -F .run-unit/unit465-msg2.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
