#!/bin/sh
# unit 460 - derive this unit's helper scripts from 459's: the name and the entry commit change, nothing else.
cd /c/Source/HamLet/.run-unit || exit 1
for f in build cf run round text textrun textsave textsort commit tx validate tick clock gitstate v11 exit-print
do
  sed -e 's/unit459/unit460/g' -e 's/19109b51/1f1c915d/g' unit459-$f.sh > unit460-$f.sh
done
ls unit460-*
