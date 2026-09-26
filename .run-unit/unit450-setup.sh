#!/bin/sh
# unit 450 - copy 449's helper scripts under 450's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run commit tick percond over25 validate text textrun textsort v11 cmp pitchcmp alone show keep; do
  sed "s/unit449/unit450/g; s/unit 449/unit 450/g" .run-unit/unit449-$n.sh > .run-unit/unit450-$n.sh
done
ls .run-unit/unit450-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
