#!/bin/sh
# unit 444 - task 2 commit: the not-kept diff, its printouts and the metrics.md record. src untouched.
cd /c/Source/HamLet || exit 1
git status --short -- src
sh tools/status.sh EXECUTING "2 of 3" code none "Dropout rule not kept (17:37 boundaries 7 to 8, V-11 fails on 3); committing its diff and record"
sh .run-unit/unit444-commit.sh .run-unit/unit444-msg2.txt PROJECT_STATUS.md docs/phase-requirements/metrics.md .run-unit/unit444-*.txt .run-unit/unit444-*.sh .run-unit/unit444-*.md .run-unit/unit444-wbe-notkept.diff
