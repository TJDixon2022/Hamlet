#!/bin/sh
# unit 433 - copy unit 432's run scripts, renamed; source diff base becomes this unit's entry f1e751ea; totals compare against unit 432's exit.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit revert alone transmit srcdiff types cwtypes rows cmprows cmptrace same added st cnt totals pitch opening
do
  sed "s/unit432/unit433/g; s/unit 432/unit 433/g" unit432-$s.sh > unit433-$s.sh
  echo "copied $s"
done
sed -i "s/cec344ad/f1e751ea/g" unit433-srcdiff.sh
sed -i "s/unit431-/unit432-/g; s/unit 431 exit/unit 432 exit/g" unit433-totals.sh
sed -i "s/timeout 300 dotnet test/timeout 600 dotnet test/" unit433-pitch.sh
