#!/bin/sh
# unit 449 - V-11 per recording from two metrics printouts, on every requirement metric:
# MET-CER-SURE may not rise, wrong-or-added (MET-INVENTED's count) may not rise,
# sure-and-right (MET-COVERAGE's count) may not fall, word boundaries wrong (MET-WBE's count) may not rise.
# Usage: sh .run-unit/unit449-v11.sh <before.txt> <after.txt>
cd /c/Source/HamLet || exit 1
pick() {
  grep "^ *row | " "$1" | grep -v "row | recording" | awk -F' [|] ' '$5 ~ /^[0-9]+$/ && NF >= 17 { cer = ($8 ~ /^[0-9.]+$/) ? $8 : "-"; print $2 "\t" $6+$7 "\t" cer "\t" $10 "\t" $15+$16 }' | sort
}
pick "$1" > .run-unit/unit449-v11-a.tmp
pick "$2" > .run-unit/unit449-v11-b.tmp
join -t "	" .run-unit/unit449-v11-a.tmp .run-unit/unit449-v11-b.tmp | awk -F'\t' '
  { n++;
    worse = ($6 > $2) || ($3 != "-" && $7 != "-" && $7 > $3) || ($8 < $4) || ($9 > $5);
    printf "%s  wrong-or-added %s -> %s  CER-SURE %s -> %s  right %s -> %s  boundaries wrong %s -> %s%s\n", $1, $2, $6, $3, $7, $4, $8, $5, $9, (worse ? "  WORSE" : "");
    if (worse) bad++ }
  END { printf "recordings compared %d, worse %d\n", n, bad+0 }'
