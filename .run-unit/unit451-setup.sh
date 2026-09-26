#!/bin/sh
# unit 451 - copy 450's helper scripts under 451's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run commit tick percond over25 validate text textrun textsort v11 cmp pitchcmp alone show keep; do
  sed "s/unit450/unit451/g; s/unit 450/unit 451/g" .run-unit/unit450-$n.sh > .run-unit/unit451-$n.sh
done
ls .run-unit/unit451-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
