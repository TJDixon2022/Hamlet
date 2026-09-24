#!/bin/sh
# unit 415 - put a taken-out change back in the working tree, uncommitted, to be narrowed.
# Usage: sh .run-unit/unit415-reapply.sh <take-out commit>
cd /c/Source/HamLet || exit 1
git revert --no-commit "$1" || exit 1
git status --short src
