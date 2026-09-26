#!/bin/sh
# unit 445 - stage the named paths, commit with the message file, push.
# Usage: sh .run-unit/unit445-commit.sh <message-file> <path>...
cd /c/Source/HamLet || exit 1
MSG=$1
shift
git add -- "$@" || exit 1
git commit -q -F "$MSG" || exit 1
git log --oneline -1
git push -q origin main 2>&1 | tail -3
git status --short -- src tests | head
