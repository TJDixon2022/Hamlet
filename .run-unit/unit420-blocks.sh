#!/bin/sh
# unit 420 - table every map block: band, shortName, lowHz, highHz, family.
cd /c/Source/HamLet || exit 1
awk '
/"band":/ { gsub(/[",]/, "", $2); band = $2 " " $3; gsub(/[",]/, "", band) }
/"shortName":/ { s = $0; sub(/.*"shortName": "/, "", s); sub(/".*/, "", s); sn = s }
/"lowHz":/ { lo = $2; gsub(/,/, "", lo) }
/"highHz":/ { hi = $2; gsub(/,/, "", hi) }
/"family":/ { f = $2; gsub(/[",]/, "", f); printf "%-8s %-6s %-10s %10s %10s\n", band, f, sn, lo, hi }
' data/bands/us-neighborhoods.json > .run-unit/unit420-blocks.txt
echo "all cw-family blocks:"
grep " cw " .run-unit/unit420-blocks.txt
echo "count by shortName in the cw family:"
grep " cw " .run-unit/unit420-blocks.txt | awk '{ print $4 }' | sort | uniq -c
echo "shortName CW DX blocks:"
grep " cw  *CW DX" .run-unit/unit420-blocks.txt
