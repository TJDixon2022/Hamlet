#!/bin/sh
# unit 432 - copy unit 431's run scripts, renamed; task count becomes 3; source diff base becomes this unit's entry cec344ad.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit revert alone transmit srcdiff types cwtypes rows cmprows cmptrace same added st cnt totals
do
  sed "s/unit431/unit432/g; s/unit 431/unit 432/g; s/of 4/of 3/g" unit431-$s.sh > unit432-$s.sh
  echo "copied $s"
done
sed -i "s/242168fc/cec344ad/g" unit432-srcdiff.sh
sed -i "s/unit430-/unit431-/g; s/unit 430 exit/unit 431 exit/g" unit432-totals.sh
