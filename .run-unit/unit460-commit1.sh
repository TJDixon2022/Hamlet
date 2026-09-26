#!/bin/sh
# unit 460 - task 1 commit: the re-banked rows and the floors file; then push.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "2 of 4" code none "Task 1 committed, named 13 of 13 and the five green; task 2 applying 459's pair speed to the working tree to trace which windows it helped and which it broke"
sh .run-unit/unit460-commit.sh .run-unit/unit460-msg1.txt tests/Hamlet.RadioEngine.Tests/Cw/TheNumberCannotBeGamedTests.cs PROJECT_STATUS.md .run-unit/unit460-*
git status --short
