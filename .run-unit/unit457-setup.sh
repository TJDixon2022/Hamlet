#!/bin/sh
# unit 457 - copy 456's helper scripts under 457's names, status first.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 5" code none "Gate passed (Hamlet confirmed); copying 456's helper scripts, then the record edits and the entry round"
for n in build cf round run commit clock text textrun textsave textsort tx tick validate exit-print; do
  sed "s/unit456/unit457/g; s/unit 456/unit 457/g; s/40de86e2/f20adf82/g" .run-unit/unit456-$n.sh > .run-unit/unit457-$n.sh
done
ls .run-unit/unit457-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
