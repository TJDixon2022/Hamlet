#!/bin/sh
# unit 430 - copy unit 429's run scripts, renamed; source diff base becomes 003f2c98.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit alone transmit srcdiff types cwtypes rows cmprows cmptrace same added st
do
  sed "s/unit429/unit430/g; s/unit 429/unit 430/g; s/of 4/of 3/g" unit429-$s.sh > unit430-$s.sh
  echo "copied $s"
done
sed -i "s/f94886a1/003f2c98/g" unit430-srcdiff.sh
