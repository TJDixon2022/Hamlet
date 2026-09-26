#!/bin/sh
# unit 444 - copy 443's helper scripts under 444's names, and check the instruction's commits.
cd /c/Source/HamLet || exit 1
for n in build cf round run cmp commit tick v11 text; do
  sed "s/unit443/unit444/g; s/unit 443/unit 444/g" .run-unit/unit443-$n.sh > .run-unit/unit444-$n.sh
done
ls .run-unit/unit444-*
grep -n "WORK_INSTRUCTION\|CURRENT_STEP" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
for c in f14b2453 42d5dbb9 14f515bd; do git merge-base --is-ancestor $c HEAD && echo "$c ancestor" || echo "$c NOT ancestor"; done
git log --oneline -1 f14b2453
git log --oneline -1 42d5dbb9
git log --oneline -1 14f515bd
git diff --stat
git diff RUN_LEDGER.md PHASE_STATUS.md | head -60
