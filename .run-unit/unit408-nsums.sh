#!/bin/sh
# unit 408 - totals of a named table: named, named elements, placeholders, emitted.
# Usage: sh .run-unit/unit408-nsums.sh <outname>...
cd /c/Source/HamLet || exit 1
for r in "$@"
do
  awk -v r="$r" '{ n += $2; e += $3; p += $4; t += $5 } END { print r ": named " n " elements " e " placeholders " p " emitted " t }' ".run-unit/unit408-$r-ntable.txt"
done
echo "== per row: name | t1 named ph | b2 named ph | gate named ph"
join .run-unit/unit408-cap-t1-ntable.txt .run-unit/unit408-cap-b2-ntable.txt | join - .run-unit/unit408-cap-gate-ntable.txt \
  | awk '{ print $1 " | " $2 " " $4 " | " $7 " " $9 " | " $12 " " $14 }'
