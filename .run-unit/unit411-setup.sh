#!/bin/sh
# unit 411 - copy unit 410's runner scripts under the unit 411 name.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit same round
do
  sed "s/unit410/unit411/g; s/unit 410/unit 411/g; s/of 4>/of 4>/g" .run-unit/unit410-$n.sh > .run-unit/unit411-$n.sh
done
ls .run-unit/unit411*
git rev-parse HEAD
