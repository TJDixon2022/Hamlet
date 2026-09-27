#!/bin/sh
# unit 463 - copy 462's generic helpers under 463's name.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commit commitrun textsave portsave judge paritykeep tx gitstate validate v11 status screen
do
  sed 's/unit462/unit463/g' unit462-$n.sh > unit463-$n.sh
done
ls unit463-*
