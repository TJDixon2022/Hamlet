#!/bin/sh
# unit 441 - V-11 per recording from two metrics printouts: wrong-or-added may not rise, sure-and-right may not fall.
# Usage: sh .run-unit/unit441-v11.sh <before.txt> <after.txt>
cd /c/Source/HamLet || exit 1
grep "^ *row | " "$1" | grep -v "row | recording" | awk -F' [|] ' '$5 ~ /^[0-9]+$/ { print $2 "\t" $6+$7 "\t" $10 }' | sort > .run-unit/unit441-v11-a.tmp
grep "^ *row | " "$2" | grep -v "row | recording" | awk -F' [|] ' '$5 ~ /^[0-9]+$/ { print $2 "\t" $6+$7 "\t" $10 }' | sort > .run-unit/unit441-v11-b.tmp
join -t "	" .run-unit/unit441-v11-a.tmp .run-unit/unit441-v11-b.tmp | awk -F'\t' '
  { n++; if ($2 != $4 || $3 != $5) printf "%s  wrong-or-added %s -> %s  right %s -> %s%s\n", $1, $2, $4, $3, $5, (($4 > $2 || $5 < $3) ? "  WORSE" : "") ;
    if ($4 > $2 || $5 < $3) bad++ }
  END { printf "recordings compared %d, worse %d\n", n, bad+0 }'
