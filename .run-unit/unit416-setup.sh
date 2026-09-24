#!/bin/sh
# unit 416 - copy unit 415's runner scripts under the unit 416 name.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round rows diff srcdiff
do
  sed "s/unit415/unit416/g; s/unit 415/unit 415/g" .run-unit/unit415-$n.sh > .run-unit/unit416-$n.sh
done
ls .run-unit/unit416*
grep -E "WORK_INSTRUCTION|CURRENT_STEP" PHASE_STATUS.md
grep -n "<Version>" Directory.Build.props
git rev-parse HEAD
