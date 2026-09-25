#!/bin/sh
# unit 426 - copy unit 424's run scripts, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit transmit srcdiff fullbuild types rig rigsum
do
  sed "s/unit424/unit426/g; s/unit 424/unit 426/g" unit424-$s.sh > unit426-$s.sh
  echo "copied $s"
done
sed -i "s/e4085d43/972510e6/g" unit426-srcdiff.sh
grep -n "972510e6" unit426-srcdiff.sh
