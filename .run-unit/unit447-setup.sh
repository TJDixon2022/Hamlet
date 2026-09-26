#!/bin/sh
# unit 447 - copy 446's helper scripts under 447's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run cmp commit tick v11 text textrun percond show; do
  sed "s/unit446/unit447/g; s/unit 446/unit 447/g; s/step 5 in both/step 4 in both/; s/criterion of step 5/criterion of step 4/; s/of 3>/of 4>/" .run-unit/unit446-$n.sh > .run-unit/unit447-$n.sh
done
ls .run-unit/unit447-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
