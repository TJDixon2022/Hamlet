#!/bin/sh
# unit 441 task 1 - PROJECT_STATUS.md is this unit's; clear the pick state, keep G1 staged.
cd /c/Source/HamLet || exit 1
cp .run-unit/unit441-status.keep PROJECT_STATUS.md
git reset -q -- PROJECT_STATUS.md
git cherry-pick --quit 2>/dev/null
ls .git/CHERRY_PICK_HEAD 2>/dev/null
git status --short -- PROJECT_STATUS.md src tests
head -10 PROJECT_STATUS.md | grep -c "<<<<"
