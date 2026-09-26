#!/bin/sh
# unit 453 - copy 452's helper scripts under 453's names.
cd /c/Source/HamLet || exit 1
for n in build cf round run commit tick percond perrec over25 validate text textrun textsort v11 cmp alone show keep; do
  sed "s/unit452/unit453/g; s/unit 452/unit 453/g" .run-unit/unit452-$n.sh > .run-unit/unit453-$n.sh
done
ls .run-unit/unit453-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
