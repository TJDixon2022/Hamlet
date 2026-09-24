#!/bin/sh
# unit 424 - copy unit 423's run scripts and unit 420's Rig round scripts, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit alone transmit srcdiff fullbuild types
do
  sed "s/unit423/unit424/g; s/unit 423/unit 424/g" unit423-$s.sh > unit424-$s.sh
  echo "copied $s from 423"
done
for s in rig rigsum
do
  sed "s/unit420/unit424/g; s/unit 420/unit 424/g; s/of 3>/of 4>/g" unit420-$s.sh > unit424-$s.sh
  echo "copied $s from 420"
done
sed -i "s/0456cad7/e4085d43/g" unit424-srcdiff.sh
sed -i "s/TASK 3 of 3/TASK 4 of 4/" unit424-fullbuild.sh
grep -n "e4085d43\|TASK" unit424-srcdiff.sh unit424-fullbuild.sh
