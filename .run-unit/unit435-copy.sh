#!/bin/sh
# unit 435 - copy unit 433's run scripts, renamed; source diff base becomes this unit's entry 8555034d; totals compare against unit 433's exit.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit revert alone transmit srcdiff types cwtypes rows cmprows cmptrace same added st cnt totals pitch opening
do
  sed "s/unit433/unit435/g; s/unit 433/unit 435/g" unit433-$s.sh > unit435-$s.sh
  echo "copied $s"
done
sed -i "s/f1e751ea/8555034d/g" unit435-srcdiff.sh
sed -i "s/unit432-/unit433-/g; s/unit 432 exit/unit 433 exit/g" unit435-totals.sh
git -C /c/Source/HamLet rev-parse --short=8 HEAD
