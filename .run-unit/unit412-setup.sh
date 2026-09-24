#!/bin/sh
# unit 412 - copy unit 411's runner scripts under the unit 412 name.
cd /c/Source/HamLet || exit 1
for n in run cf commit build transmit round
do
  sed "s/unit411/unit412/g; s/unit 411/unit 412/g" .run-unit/unit411-$n.sh > .run-unit/unit412-$n.sh
done
ls .run-unit/unit412*
git rev-parse HEAD
