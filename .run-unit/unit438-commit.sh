#!/bin/sh
# unit 438 - stage the tracked changes and this unit's scripts and outputs, commit with the given subject, push.
# Usage: sh .run-unit/unit438-commit.sh "<subject>"
cd /c/Source/HamLet || exit 1
git add -u
git add .run-unit/unit438-*
git commit -q -m "$1" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
git log --oneline -1
git push -q origin main 2>&1 | tail -3
echo "PUSH RC=$?"
git status -sb | head -1
