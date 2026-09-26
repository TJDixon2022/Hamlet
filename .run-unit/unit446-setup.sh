#!/bin/sh
# unit 446 - copy 445's helper scripts under 446's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run cmp commit tick v11 text percond; do
  sed "s/unit445/unit446/g; s/unit 445/unit 446/g; s/step 3 in both/step 5 in both/; s/criterion of step 3/criterion of step 5/" .run-unit/unit445-$n.sh > .run-unit/unit446-$n.sh
done
ls .run-unit/unit446-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
