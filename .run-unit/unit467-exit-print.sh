#!/bin/sh
# unit 466 task 4 - the exit prints: the port, the eleven transmit files, git state and this unit's commits.
cd /c/Source/HamLet || exit 1
sh .run-unit/unit467-portdiff.sh
sh .run-unit/unit467-tx.sh
echo "== git status"
git status --short
echo "== this unit's commits"
git log --oneline 9cc2c74b..HEAD
