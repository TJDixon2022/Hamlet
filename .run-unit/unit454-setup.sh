#!/bin/sh
# unit 454 - copy 453's helper scripts under 454's names, status first.
cd /c/Source/HamLet || exit 1
sh tools/status.sh EXECUTING "0 of 4" code none "Gate passed; reading section M, the harness and the scorer before the entry round"
for n in build cf round run commit; do
  sed "s/unit453/unit454/g; s/unit 453/unit 454/g; s/of 3/of 4/g" .run-unit/unit453-$n.sh > .run-unit/unit454-$n.sh
done
ls .run-unit/unit454-*
date "+%Y-%m-%dT%H:%M:%S%:z"
git rev-parse HEAD
