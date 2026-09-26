#!/bin/sh
# unit 448 - pull the MET-PITCH-ERR table and the opening lines out of one printout pair.
# Usage: sh .run-unit/unit448-table.sh <suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
OUT=.run-unit/unit448-pitch-table-$1.txt
grep -a "^ *capture | file" .run-unit/unit448-pitch-$1.txt | head -1 | sed -E "s/^ +//" > "$OUT"
grep -a "^ *capture | .* | >25\$" .run-unit/unit448-pitch-$1.txt | sed -E "s/^ +//" | sort -u >> "$OUT"
grep -a "^ *capture | .* | windows >25\$" .run-unit/unit448-pitch-$1.txt | sed -E "s/^ +//" | sort -u >> "$OUT"
grep -a "^ *capture | .* | -\$" .run-unit/unit448-pitch-$1.txt | sed -E "s/^ +//" | sort -u >> "$OUT"
grep -a "^ *total |\|^ *absent |" .run-unit/unit448-pitch-$1.txt | sed -E "s/^ +//" | sort -u >> "$OUT"
grep -a "^ *join |\|^ *opening |" .run-unit/unit448-opening-$1.txt | sed -E "s/^ +//" | sort -u >> "$OUT"
wc -l "$OUT"
