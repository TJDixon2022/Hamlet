#!/bin/sh
# unit 449 - copy 448's helper scripts under 449's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run commit tick percond over25 validate text textrun textsort v11 table cmp pitchcmp; do
  sed "s/unit448/unit449/g; s/unit 448/unit 449/g" .run-unit/unit448-$n.sh > .run-unit/unit449-$n.sh
done
ls .run-unit/unit449-*
grep -n "pitch" .run-unit/unit447-round.sh .run-unit/unit448-*.sh | cut -c1-240
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
