#!/bin/sh
# unit 428 - copy unit 425's run scripts, renamed; the source diff against entry 9db61107.
cd /c/Source/HamLet/.run-unit || exit 1
for s in build cf round run commit alone transmit srcdiff fullbuild types cwtypes entry rows cmprows cmptrace
do
  sed "s/unit425/unit428/g; s/unit 425/unit 428/g" unit425-$s.sh > unit428-$s.sh
  echo "copied $s"
done
sed -i "s/b12cbbe4/9db61107/g" unit428-srcdiff.sh
sed -i "s/the eight added single-element letters/the added single-element letters/" unit428-entry.sh
