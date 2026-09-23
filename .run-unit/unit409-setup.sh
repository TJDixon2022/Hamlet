#!/bin/sh
# unit 409 - copy unit 408's runner scripts under the unit 409 name.
cd /c/Source/HamLet || exit 1
for n in run cf types commit build transmit
do
  sed "s/unit408/unit409/g; s/unit 408/unit 409/g" .run-unit/unit408-$n.sh > .run-unit/unit409-$n.sh
done
sed "s/unit406/unit409/g; s/unit 406/unit 409/g" .run-unit/unit406-list.sh > .run-unit/unit409-list.sh
sed "s/unit406/unit409/g; s/unit 406/unit 409/g" .run-unit/unit406-readers.sh > .run-unit/unit409-readers.sh
ls .run-unit/unit409*
