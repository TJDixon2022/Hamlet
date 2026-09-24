#!/bin/sh
# unit 415 - every capture row's named count, entry beside a later run, from the captures type's output.
# Usage: sh .run-unit/unit416-rows.sh <later-suffix>
cd /c/Source/HamLet || exit 1
E=.run-unit/unit416-captures-entry.txt
L=.run-unit/unit416-captures-$1.txt
grep -a -E "^ (unadjudicated/)?cw-[0-9-]+: [0-9]+ named against" $E | sed -E "s/^ //; s/: ([0-9]+) named against a floor of [0-9]+, ([0-9]+) named elements.*/ \1 \2/" | sort > .run-unit/unit416-rows-entry.txt
grep -a -E "^ (unadjudicated/)?cw-[0-9-]+: [0-9]+ named against" $L | sed -E "s/^ //; s/: ([0-9]+) named against a floor of [0-9]+, ([0-9]+) named elements.*/ \1 \2/" | sort > .run-unit/unit416-rows-$1.txt
join .run-unit/unit416-rows-entry.txt .run-unit/unit416-rows-$1.txt | awk '{ m = ($2 != $4 || $3 != $5) ? "  moved" : ""; print $1, "named", $2, "->", $4, "elements", $3, "->", $5 m }' > .run-unit/unit416-rows-cmp-$1.txt
echo "rows printed at entry: $(wc -l < .run-unit/unit416-rows-entry.txt), in $1: $(wc -l < .run-unit/unit416-rows-$1.txt)"
grep moved .run-unit/unit416-rows-cmp-$1.txt
echo "== rows not printed in $1, their failure messages"
grep -a -E "fell from" $L | sed -E "s/^ +//"
