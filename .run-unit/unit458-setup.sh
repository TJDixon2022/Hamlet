#!/bin/sh
# unit 458 - derive this unit's helper scripts from 457's: the name, the task count and the entry commit change, nothing else.
cd /c/Source/HamLet/.run-unit || exit 1
for f in build cf run round text textrun textsave textsort commit tx validate tick clock exit-print
do
  sed -e 's/unit457/unit458/g' -e 's/of 5/of 4/g' -e 's/f20adf82/845fd70f/g' unit457-$f.sh > unit458-$f.sh
done
ls unit458-*
