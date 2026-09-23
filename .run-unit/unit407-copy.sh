#!/bin/sh
# unit 407 - copy unit 406's helper scripts under unit 407's names.
cd /c/Source/HamLet/.run-unit || exit 1
for f in build run list nums transmit putback commit gate cf table readers
do
  sed 's/unit406/unit407/g; s/unit 406/unit 407/g' unit406-$f.sh > unit407-$f.sh
done
# the entry capture table is compared against unit 406's exit table
sed -i 's#unit407-cap-entry-table.txt#unit407-cap-entry-table.txt#; s#.run-unit/unit405-cap-exit-table.txt#.run-unit/unit406-cap-exit-table.txt#' unit407-gate.sh
ls unit407-*.sh
grep -n "cap-exit-table\|cap-entry-table" unit407-gate.sh
