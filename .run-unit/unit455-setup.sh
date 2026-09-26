#!/bin/sh
# unit 455 - copy 454's and 453's helper scripts under 455's names, status first.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 4" code none "Gate passed; copying the helper scripts, then the record edits and the entry round"
for n in build cf round run commit clock; do
  sed "s/unit454/unit455/g; s/unit 454/unit 455/g" .run-unit/unit454-$n.sh > .run-unit/unit455-$n.sh
done
for n in text textrun textsort; do
  sed "s/unit453/unit455/g; s/unit 453/unit 455/g; s/of 3/of 4/g" .run-unit/unit453-$n.sh > .run-unit/unit455-$n.sh
done
ls .run-unit/unit455-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
