#!/bin/sh
# unit 442 - copy 441's helper scripts under 442's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run commit v11; do
  sed "s/unit441/unit442/g; s/unit 441/unit 442/g" .run-unit/unit441-$n.sh > .run-unit/unit442-$n.sh
done
ls .run-unit/unit442-*
grep -n "WORK_INSTRUCTION" PHASE_STATUS.md docs/phase-requirements/PHASE_STATUS.md
grep -n "Version>" Directory.Build.props
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
