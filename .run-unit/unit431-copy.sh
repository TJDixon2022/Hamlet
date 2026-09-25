#!/bin/sh
# unit 431 - copy unit 430's run scripts, renamed; source diff base becomes 242168fc.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit revert alone transmit srcdiff types cwtypes rows cmprows cmptrace same added st cnt totals entry
do
  sed "s/unit430/unit431/g; s/unit 430/unit 431/g; s/of 3/of 4/g" unit430-$s.sh > unit431-$s.sh
  echo "copied $s"
done
sed -i "s/003f2c98/242168fc/g" unit431-srcdiff.sh
