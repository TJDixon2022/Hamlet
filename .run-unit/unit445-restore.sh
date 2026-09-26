#!/bin/sh
# unit 445 - restore .run-unit/PROJECT_STATUS.md, which the session overwrote by running status.sh from .run-unit at task 1's start.
cd /c/Source/HamLet || exit 1
git log --oneline -2 -- .run-unit/PROJECT_STATUS.md
git diff --stat -- .run-unit/PROJECT_STATUS.md
git checkout -- .run-unit/PROJECT_STATUS.md
git status --short -- .run-unit/PROJECT_STATUS.md
echo restored
