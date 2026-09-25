#!/bin/sh
# unit 438 - every capture row's above-bar characters and elements, entry beside re-mix.
cd /c/Source/HamLet/.run-unit || exit 1
for f in entry local
do
  grep -E "^ [^ ]+: [0-9]+ named, [0-9]+ at or above the span bar" unit438-captures-$f.txt \
    | sed -E 's/^ ([^ ]+): [0-9]+ named, ([0-9]+) at or above.*; [0-9]+ named elements, ([0-9]+) above the bar.*/\1 \2 \3/' \
    | sort -u > unit438-rows-$f.tmp
done
join unit438-rows-entry.tmp unit438-rows-local.tmp | awk '{ mark = ($4 < $2 || $5 < $3) ? "FALLS" : (($4 != $2 || $5 != $3) ? "moves" : "same"); printf "%s | chars %s -> %s | elements %s -> %s | %s\n", $1, $2, $4, $3, $5, mark }' > unit438-rows-table.txt
wc -l < unit438-rows-table.txt
grep -c FALLS unit438-rows-table.txt
grep -c moves unit438-rows-table.txt
grep -c same unit438-rows-table.txt
