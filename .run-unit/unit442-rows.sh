#!/bin/sh
# unit 442 - one line per floor row that is red at HEAD: row, banked count, count at HEAD, text before and after.
# Usage: sh .run-unit/unit442-rows.sh <before.txt> <head.txt> [all]
cd /c/Source/HamLet || exit 1
grep "^ *floor | " "$1" | grep -v "floor | table" | sed 's/^ *//' | awk -F' [|] ' '{ print $2 "|" $3 "\t" $7 "\t" $8 }' | sort > .run-unit/unit442-rows-a.tmp
grep "^ *floor | " "$2" | grep -v "floor | table" | sed 's/^ *//' | awk -F' [|] ' '{ print $2 "|" $3 "\t" $4 "\t" $5 "\t" $6 "\t" $7 "\t" $8 }' | sort > .run-unit/unit442-rows-b.tmp
join -t "	" .run-unit/unit442-rows-a.tmp .run-unit/unit442-rows-b.tmp | awk -F'\t' -v all="$3" '
  { red = ($5 + 0 < $4 + 0) || ($6 != "-" && $7 + 0 < $6 + 0);
    if (red || all == "all")
      printf "%s | named banked %s, at HEAD %s | elements banked %s, at HEAD %s%s\n    before: %s\n    after:  %s\n", $1, $4, $5, $6, $7, (red ? " | RED" : ""), $3, $8 }'
