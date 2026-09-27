#!/bin/sh
# unit 471 - copy unit 470's helpers under 471 names, renaming the unit inside them.
cd /c/Source/HamLet/.run-unit || exit 1
for n in run build cf round extra save v11 paritykeep alone portdiff gitstate validate
do
  [ -f unit470-$n.sh ] || { echo "missing unit470-$n.sh"; continue; }
  sed -e 's/unit470/unit471/g' -e 's/"<n of 3>"/"<n of 4>"/' unit470-$n.sh > unit471-$n.sh
  echo "unit471-$n.sh"
done
grep -l unit470 unit471-*.sh
echo "(end of files still naming 470)"
