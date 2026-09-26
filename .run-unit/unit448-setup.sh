#!/bin/sh
# unit 448 - copy 447's helper scripts under 448's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run cmp commit tick v11 text textrun percond pitchcmp table; do
  sed "s/unit447/unit448/g; s/unit 448/unit 448/g; s/of 4>/of 3>/" .run-unit/unit447-$n.sh > .run-unit/unit448-$n.sh
done
sed -i "s/unit 448/unit 448/g" .run-unit/unit448-*.sh
ls .run-unit/unit448-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
