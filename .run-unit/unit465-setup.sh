#!/bin/sh
# unit 465 - copy 464's generic helpers under 465's name.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build round cf commit commitrun textsave portsave paritykeep tx gitstate validate v11 classes
do
  sed 's/unit464/unit465/g' unit464-$n.sh > unit465-$n.sh
done
ls unit465-*
head -3 unit464-text-before-classes.txt
head -3 unit464-port-before.txt
head -3 unit464-text-before.sorted.txt
wc -l unit464-text-before-classes.txt unit464-port-before.txt unit464-text-before.sorted.txt
