#!/bin/sh
# unit 439 - copies unit 438's run scripts to unit 439 names, with the unit number changed.
cd /c/Source/HamLet/.run-unit || exit 1
for n in ps build run cf round rows same adjdiff floors perread commit exitdiff exitcmp revert tracerows
do
  sed 's/unit438/unit439/g; s/unit 438/unit 439/g' unit438-$n.sh > unit439-$n.sh
done
sed -i 's/local/bridge/g' unit439-rows.sh unit439-adjdiff.sh unit439-floors.sh unit439-perread.sh
ls unit439-*.sh
