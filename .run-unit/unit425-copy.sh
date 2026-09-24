#!/bin/sh
# unit 425 - copy unit 424's run scripts and unit 421's row and trace comparisons, renamed.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit alone transmit srcdiff fullbuild types
do
  sed "s/unit424/unit425/g; s/unit 424/unit 425/g" unit424-$s.sh > unit425-$s.sh
  echo "copied $s from 424"
done
for s in rows cmprows cmptrace
do
  sed "s/unit421/unit425/g; s/unit 421/unit 425/g" unit421-$s.sh > unit425-$s.sh
  echo "copied $s from 421"
done
sed -i "s/e4085d43/b12cbbe4/g" unit425-srcdiff.sh
grep -n "b12cbbe4\|TASK" unit425-srcdiff.sh unit425-fullbuild.sh
cat unit425-srcdiff.sh unit425-fullbuild.sh unit425-cmprows.sh unit425-cmptrace.sh
