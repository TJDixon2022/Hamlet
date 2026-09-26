#!/bin/sh
# unit 451 - from one MET-PITCH-ERR printout, the totals and the files more than 25 Hz off.
# Usage: sh .run-unit/unit451-over25.sh <suffix>
cd /c/Source/HamLet || exit 1
export LC_ALL=C
F=.run-unit/unit451-pitch-$1.txt
grep -a "^ *total |\|^ *absent |" "$F" | sed -E "s/^ +//" | sort -u | cut -c1-240
grep -a "^ *capture | .* | >25\$" "$F" | sed -E "s/^ +//" | sort -u | cut -c1-160
echo "files more than 25 Hz off: $(grep -a "^ *capture | .* | >25\$" "$F" | sed -E "s/^ +//" | sort -u | wc -l)"
