#!/bin/sh
# unit 476 - make this unit's helper scripts from unit 475's: names, work instruction, rules line, entry commit.
cd /c/Source/HamLet/.run-unit || exit 1
for f in status commit build cf run close exit-print
do
  sed -e 's/unit475/unit476/g' -e 's/unit 475/unit 476/g' \
      -e "s/475 - the swing bar the owner's ear says is wrong/476 - the oscilloscope: a mark is the envelope over a threshold, at any pitch/" \
      -e 's/HM-DEC-184 (2026-09-27)/HM-DEC-185 (2026-09-28)/' \
      -e 's/62e272e6/ba7ef00a/g' -e 's/of 3>/of 4>/' \
      unit475-$f.sh > unit476-$f.sh
done
grep -n "476\|185\|ba7ef" unit476-status.sh unit476-exit-print.sh | cut -c1-140 | head
