#!/bin/sh
# unit 456 - copy 455's helper scripts under 456's names, status first.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 5" code none "Gate passed; copying the helper scripts, then the record edits and the entry round"
for n in build cf round run commit clock text textrun textsave textsort tx tick validate; do
  sed "s/unit455/unit456/g; s/unit 455/unit 456/g; s/of 4/of 5/g" .run-unit/unit455-$n.sh > .run-unit/unit456-$n.sh
done
ls .run-unit/unit456-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
