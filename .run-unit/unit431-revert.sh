#!/bin/sh
# unit 431 - take a change back out by a revert commit, with the judgment appended to the revert's message, and push.
# Usage: sh .run-unit/unit431-revert.sh <sha> <message-file> "<TASK n of 4>" "<note>" <extra path>...
cd /c/Source/HamLet || exit 1
SHA=$1
MSG=$2
TK=$3
NT=$4
shift 4
git revert --no-edit --no-commit "$SHA" || exit 1
git add -- "$@" || exit 1
git commit -q -F "$MSG" || exit 1
git log --oneline -1
git push -q origin main
echo "push rc $?"
sh tools/status.sh EXECUTING "$TK" code none "$NT"
