#!/bin/sh
# unit 470 - task 1 commit: the fact, the trace and its printouts; never anything under src; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "1 of 3" code none "Task 1 done - the gate would cost up to 293 right real letters for 29 wrong; rule registered before any after-figure; committing the fact and trace"
git add -- PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/WhatTheSureLettersWerePrintedUnderFact.cs || exit 1
for f in .run-unit/unit470-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit470-msg1.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
