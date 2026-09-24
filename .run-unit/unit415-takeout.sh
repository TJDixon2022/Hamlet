#!/bin/sh
# unit 415 - take a change out in a commit of its own, confirm src matches entry HEAD, push.
# Usage: sh .run-unit/unit415-takeout.sh <commit> <message-file> "<TASK n of 4>" "<note>"
cd /c/Source/HamLet || exit 1
git revert --no-commit "$1" || exit 1
git status --short src
echo "== src against 61d8b58f"
git diff --stat 61d8b58f -- src
echo "== end src"
git commit -q -F "$2" || exit 1
git log --oneline -1
git show --stat --format= HEAD
git push -q origin main
echo "push rc $?"
sh tools/status.sh EXECUTING "$3" code none "$4"
