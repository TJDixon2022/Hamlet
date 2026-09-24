#!/bin/sh
# unit 424 - stage named paths, commit with the message file, push, write status.
# Usage: sh .run-unit/unit424-commit.sh <message-file> "<TASK n of 4>" "<note>" <path>...
cd /c/Source/HamLet || exit 1
MSG=$1
TK=$2
NT=$3
shift 3
git add -- "$@" || exit 1
git commit -q -F "$MSG" || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
sh tools/status.sh EXECUTING "$TK" code none "$NT"
