#!/bin/sh
# unit 427 - run one app type against the committed tree (task 1's work stashed), then restore.
# Usage: sh .run-unit/unit427-entrycheck.sh <Type>
cd /c/Source/HamLet || exit 1
git stash push -q -- src tests || exit 1
git stash list | head -1
sh .run-unit/unit427-build.sh "entrycheck" "TASK 1 of 4" "Task 1 - checking $1 against the committed tree, work stashed"
sh .run-unit/unit427-types.sh entrycheck "TASK 1 of 4" "$1"
git stash pop -q
echo "pop rc $?"
git status --short src tests
