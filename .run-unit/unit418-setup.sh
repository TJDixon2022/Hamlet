#!/bin/sh
# unit 418 - copy unit 417's runner scripts under the unit 418 name.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round numbers
do
  sed "s/unit417/unit418/g; s/unit 417/unit 418/g" .run-unit/unit417-$n.sh > .run-unit/unit418-$n.sh
done
sed "s/unit417/unit418/g; s/unit 417/unit 418/g; s/12e2d442/3211874b/g" .run-unit/unit417-srcdiff.sh > .run-unit/unit418-srcdiff.sh
ls .run-unit/unit418*
grep -E "WORK_INSTRUCTION|CURRENT_STEP" PHASE_STATUS.md
grep -n "<Version>" Directory.Build.props
git rev-parse HEAD
