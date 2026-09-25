#!/bin/sh
# unit 437 - every capture row's above-bar characters and elements, entry beside re-mix.
cd /c/Source/HamLet/.run-unit || exit 1
for f in entry remix
do
  grep -E "^ [^ ]+: [0-9]+ named, [0-9]+ at or above the span bar" unit437-captures-$f.txt \
    | sed -E 's/^ ([^ ]+): [0-9]+ named, ([0-9]+) at or above.*; [0-9]+ named elements, ([0-9]+) above the bar.*/\1 \2 \3/' \
    | sort -u > unit437-rows-$f.tmp
done
join unit437-rows-entry.tmp unit437-rows-remix.tmp | awk '{ mark = ($4 < $2 || $5 < $3) ? "FALLS" : (($4 != $2 || $5 != $3) ? "moves" : "same"); printf "%s | chars %s -> %s | elements %s -> %s | %s\n", $1, $2, $4, $3, $5, mark }' > unit437-rows-table.txt
wc -l < unit437-rows-table.txt
grep -c FALLS unit437-rows-table.txt
grep -c moves unit437-rows-table.txt
grep -c same unit437-rows-table.txt
