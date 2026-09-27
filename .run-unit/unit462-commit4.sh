#!/bin/sh
# unit 462 - task 4 commit: the exit printouts, the fact's correction, output.md, the final status; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh COMPLETED "4 of 4" code none "Unit 462 done: C, B and W all refused under R78 and reverted (W closest - every total right, 3 recordings worse); 9.4 open; exit as entry; output.md written"
git add -- PROJECT_STATUS.md output.md tests/Hamlet.RadioEngine.Tests/Cw/WhatFldigisEdgesWouldMoveFact.cs || exit 1
for f in .run-unit/unit462-*
do
  case "$f" in
    *.tmp) ;;
    *) git add -- "$f" ;;
  esac
done
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit462-msg4.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
git status --short
