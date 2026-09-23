#!/bin/sh
# unit 410 - copy unit 409's runner scripts under the unit 410 name.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit same list round
do
  sed "s/unit409/unit410/g; s/unit 409/unit 410/g" .run-unit/unit409-$n.sh > .run-unit/unit410-$n.sh
done
ls .run-unit/unit410*
git rev-parse HEAD
