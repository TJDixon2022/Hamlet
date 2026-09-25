#!/bin/sh
# unit 441 task 1 - cherry-pick G1 unchanged, without committing; keep this unit's PROJECT_STATUS.md.
cd /c/Source/HamLet || exit 1
git status -sb | head -1
sh tools/status.sh EXECUTING "1 of 3" code none "Cherry-picking G1 from cce7985d unchanged, then judging it under R78 with the R82 coverage"
cp PROJECT_STATUS.md .run-unit/unit441-status.keep
git checkout HEAD -- PROJECT_STATUS.md
git cherry-pick -n cce7985d
echo "pick rc=$?"
git checkout HEAD -- PROJECT_STATUS.md 2>/dev/null
cp .run-unit/unit441-status.keep PROJECT_STATUS.md
git reset -q -- PROJECT_STATUS.md
git status --short | grep -v "^?? " | grep -v " .run-unit/watched\| .run-unit/approach\| .run-unit/reload\| .run-unit/step-states\| .run-unit/why"
echo "== src diff against cce7985d's own change"
git diff --cached -- src > .run-unit/unit441-g1-picked.diff
git show cce7985d -- src > .run-unit/unit441-g1-orig.diff
git diff --cached --stat -- src
