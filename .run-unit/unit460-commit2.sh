#!/bin/sh
# unit 460 - task 2 commit: the trace printer, the trace and its printouts; then push.
cd /c/Source/HamLet || exit 1
git diff --quiet -- src || { echo "src is not clean - not committing"; exit 1; }
sh tools/status.sh EXECUTING "4 of 4" code none "Task 2 committed - no pair-rule mechanism separates helped from broken, task 3 not run; task 4 exit round starting with the build"
sh .run-unit/unit460-commit.sh .run-unit/unit460-msg2.txt tests/Hamlet.RadioEngine.Tests/Cw/WhereThePairSpeedMovedTheUnitTests.cs PROJECT_STATUS.md .run-unit/unit460-*
git status --short
git status -sb | head -1
