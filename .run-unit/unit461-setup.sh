#!/bin/sh
# unit 461 - copy 460's generic helpers under 461's name.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round commit textsave tick tx gitstate validate v11
do
  sed 's/unit460/unit461/g' unit460-$n.sh > unit461-$n.sh
done
ls unit461-*
