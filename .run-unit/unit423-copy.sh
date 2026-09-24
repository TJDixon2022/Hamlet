#!/bin/sh
# unit 423 - copy unit 422's helper scripts under unit 423 names.
cd /c/Source/HamLet/.run-unit || exit 1
for f in build cf round run commit alone transmit srcdiff fullbuild types; do
  sed "s/unit422/unit423/g; s/unit 422/unit 423/g" unit422-$f.sh > unit423-$f.sh
done
ls unit423-*
for f in alone transmit srcdiff fullbuild types; do echo "=== $f"; cat unit423-$f.sh; done
