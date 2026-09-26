#!/bin/sh
# unit 443 - task 1 commit: the printer, its three printouts, the match.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit443-match.sh > .run-unit/unit443-match.txt
sh tools/status.sh EXECUTING "1 of 3" code none "Task 1 traced: 13 -> 5 (G1) -> 4 (marks speed); double-read group named; committing"
sh .run-unit/unit443-commit.sh .run-unit/unit443-msg1.txt PROJECT_STATUS.md tests/Hamlet.RadioEngine.Tests/Cw/WhereTheSureAddedLettersComeFromTests.cs .run-unit/unit443-*.txt .run-unit/unit443-*.sh
git status -sb | head -1
