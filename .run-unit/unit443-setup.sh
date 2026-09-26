#!/bin/sh
# unit 443 - copy 442's helper scripts under 443's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run cmp commit append tick v11; do
  sed "s/unit442/unit443/g; s/unit 442/unit 443/g" .run-unit/unit442-$n.sh > .run-unit/unit443-$n.sh
done
ls .run-unit/unit443-*
grep -n "WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
for c in 1fb0bad6 f14b2453 42d5dbb9 14f515bd; do git merge-base --is-ancestor $c HEAD && echo "$c ancestor" || echo "$c NOT ancestor"; done
git log --oneline -1 f14b2453
git log --oneline -1 42d5dbb9
git log --oneline -1 14f515bd
git log --oneline -1 1fb0bad6
