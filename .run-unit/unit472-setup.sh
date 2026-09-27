#!/bin/sh
# unit 472 - copy unit 471's helpers under 472 names, renaming the unit inside them.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build cf round extra save v11 paritykeep alone portdiff gitstate validate status stamp texts commit textdiff conditions exit-print
do
  [ -f unit471-$n.sh ] || { echo "missing unit471-$n.sh"; continue; }
  sed -e 's/unit471/unit472/g' unit471-$n.sh > unit472-$n.sh
  echo "unit472-$n.sh"
done
grep -l unit471 unit472-*.sh
echo "(end of files still naming 471)"
