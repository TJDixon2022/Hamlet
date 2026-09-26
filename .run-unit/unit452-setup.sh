#!/bin/sh
# unit 452 - copy 451's helper scripts under 452's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run commit tick percond over25 validate text textrun textsort v11 cmp alone show keep; do
  sed "s/unit451/unit452/g; s/unit 451/unit 452/g" .run-unit/unit451-$n.sh > .run-unit/unit452-$n.sh
done
ls .run-unit/unit452-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
