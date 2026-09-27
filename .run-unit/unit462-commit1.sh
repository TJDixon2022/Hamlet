#!/bin/sh
# unit 462 - task 1 commit: the fact, its printout, the classed texts; then push.
cd /c/Source/HamLet || exit 1
export LC_ALL=C
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
grep -a "^ text-class | " .run-unit/unit462-edges.txt | sort > .run-unit/unit462-text-before-classes.txt
wc -l .run-unit/unit462-text-before-classes.txt
sh tools/status.sh EXECUTING "1 of 4" code none "Task 1 done: C ranks first at +4 (5 fixable, 1 at risk), B -17, W -81; committing the fact and its printout"
git add -- PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/WhatFldigisEdgesWouldMoveFact.cs .run-unit/unit462-edges.txt .run-unit/unit462-text-before-classes.txt .run-unit/unit462-build-t1.txt .run-unit/unit462-build-t1b.txt .run-unit/unit462-msg1.txt .run-unit/unit462-commit1.sh || exit 1
git status --short | grep -v "^??"
git commit -q -F .run-unit/unit462-msg1.txt || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status -sb | head -1
