#!/bin/sh
# unit 438 - the 13 named floors, entry beside the local cut: recording, at or above the bar, floor.
cd /c/Source/HamLet/.run-unit || exit 1
for f in entry local
do
  grep -E "^ named \| " unit438-named-$f.txt | sed -E 's/^ named \| ([^ ]+) \| ([0-9]+) at or above the span bar against a floor of ([0-9]+) \|.*/\1 \2 \3/' | sort -u > unit438-floors-$f.tmp
done
join unit438-floors-entry.tmp unit438-floors-local.tmp | awk '{ printf "%s | floor %s | entry %s | local %s | %s\n", $1, $3, $2, $4, ($4 < $3 ? "BREAKS" : "holds") }'
