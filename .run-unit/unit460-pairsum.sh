#!/bin/sh
# unit 460 task 2 - summarise one pair-trace printout: reads, recordings, and the reads the rule would move.
# Usage: sh .run-unit/unit460-pairsum.sh <suffix>
cd /c/Source/HamLet || exit 1
F=.run-unit/unit460-pair-$1.txt
grep -a -c "^ read |" $F
grep -a "^ recording |" $F
grep -a "^ read |" $F | awk -F'|' '$15 ~ /yes/' | wc -l
grep -a "^ read |" $F | awk -F'|' '$15 ~ /yes/' | cut -c1-240
