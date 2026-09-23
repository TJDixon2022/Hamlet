#!/bin/sh
# unit 404 task 2 - revert the chain commit named, in its own commit, with the message given.
# Usage: sh .run-unit/unit404-revert.sh <commit> "<message>"
cd /c/Source/HamLet || exit 1
git revert --no-edit --no-commit "$1" || exit 1
git commit -q -m "$2" -m "Co-Authored-By: Claude Opus 5.5 (1M context) <noreply@anthropic.com>" || exit 1
git log --oneline -1 | cut -c1-70
echo "== src against entry bc2484d5"
git diff --stat bc2484d5 HEAD -- src
echo "== end"
