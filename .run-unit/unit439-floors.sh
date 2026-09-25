#!/bin/sh
# unit 439 - the 13 named floors, entry beside the bridge cut: recording, at or above the bar, floor.
cd /c/Source/HamLet/.run-unit || exit 1
for f in entry bridge
do
  grep -E "^ named \| " unit439-named-$f.txt | sed -E 's/^ named \| ([^ ]+) \| ([0-9]+) at or above the span bar against a floor of ([0-9]+) \|.*/\1 \2 \3/' | sort -u > unit439-floors-$f.tmp
done
join unit439-floors-entry.tmp unit439-floors-bridge.tmp | awk '{ printf "%s | floor %s | entry %s | bridge %s | %s\n", $1, $3, $2, $4, ($4 < $3 ? "BREAKS" : "holds") }'
