#!/bin/sh
# unit 419 - copy unit 418's run scripts under unit 419's names.
cd /c/Source/HamLet || exit 1
for n in run cf round build commit transmit
do
  sed "s/unit418/unit419/g; s/unit 418/unit 419/g" .run-unit/unit418-$n.sh > .run-unit/unit419-$n.sh
done
ls -la .run-unit/unit419-*.sh
