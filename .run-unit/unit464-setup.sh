#!/bin/sh
# unit 464 - copy 463's generic helpers under 464's name.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commit commitrun textsave portsave paritykeep tx gitstate validate v11 classes
do
  sed 's/unit463/unit464/g' unit463-$n.sh > unit464-$n.sh
done
ls unit464-*
