#!/bin/sh
# unit 417 - copy unit 416's runner scripts under the unit 417 name.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round numbers
do
  sed "s/unit416/unit417/g; s/unit 415/unit 417/g; s/TASK n of 4/TASK n of 3/g" .run-unit/unit416-$n.sh > .run-unit/unit417-$n.sh
done
sed "s/unit416/unit417/g; s/unit 415/unit 417/g; s/61d8b58f/12e2d442/g" .run-unit/unit416-srcdiff.sh > .run-unit/unit417-srcdiff.sh
ls .run-unit/unit417*
grep -E "WORK_INSTRUCTION|CURRENT_STEP" PHASE_STATUS.md
grep -n "<Version>" Directory.Build.props
git rev-parse HEAD
