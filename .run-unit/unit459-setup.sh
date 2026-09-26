#!/bin/sh
# unit 459 - derive this unit's helper scripts from 458's: the name and the entry commit change, nothing else.
cd /c/Source/HamLet/.run-unit || exit 1
for f in build cf run round text textrun textsave textsort commit tx validate tick clock status gitstate
do
  sed -e 's/unit458/unit459/g' -e 's/845fd70f/19109b51/g' unit458-$f.sh > unit459-$f.sh
done
ls unit459-*
