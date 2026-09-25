#!/bin/sh
# unit 438 - copies unit 437's run scripts to unit 438 names, with the unit number changed.
cd /c/Source/HamLet/.run-unit || exit 1
for n in build run cf round rows same adjdiff
do
  sed 's/unit437/unit438/g; s/unit 437/unit 438/g' unit437-$n.sh > unit438-$n.sh
done
sed -i 's/for f in entry remix/for f in entry local/; s/unit438-rows-remix.tmp/unit438-rows-local.tmp/' unit438-rows.sh
sed -i 's/for f in entry remix/for f in entry local/; s/entry vs remix/entry vs local/; s/the three, remix/the three, local/; s/adjudicated-remix/adjudicated-local/' unit438-adjdiff.sh
sed -i 's/Running CwUnitEstimatorTests alone, the type the rule touches/Running CwUnitEstimator tests alone, the type the change touches/' unit438-round.sh
ls unit438-*.sh
