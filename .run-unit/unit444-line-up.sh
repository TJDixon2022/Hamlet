#!/bin/sh
# unit 444 - line the two gap printouts up gap by gap on the audio clock.
# Columns: at s | length | before-G1 read's standing | before call | HEAD read's standing | HEAD call | key call | changed
cd /c/Source/HamLet || exit 1
pick() {
  grep "^gap | [0-9]" "$1" | awk -F' [|] ' '{ print $2 "\t" $3 "\t" $6 " " $7 " letter<" $8 " word<" $9 " relabel<" $10 " [" $11 "]\t" $13 "\t" $14 "\t" $12 }' | sort
}
pick .run-unit/unit444-gaps-f14b2453.table.txt > .run-unit/unit444-lineup-a.tmp
pick .run-unit/unit444-gaps-head.table.txt > .run-unit/unit444-lineup-b.tmp
join -t "	" -a 1 -a 2 -e "absent" -o 0,1.2,2.2,1.4,2.4,1.5,1.3,2.3,1.6,2.6 .run-unit/unit444-lineup-a.tmp .run-unit/unit444-lineup-b.tmp | awk -F'\t' '
  { changed = ($4 != $5) ? "CHANGED" : "";
    agrees = "";
    if (changed != "") { split($4, a, " "); split($5, b, " "); agrees = ($6 == a[1]) ? "key agrees with the old call" : ($6 == b[1]) ? "key agrees with the new call" : "key agrees with neither"; }
    printf "%s | %s / %s | before: %s | HEAD: %s | key: %s | %s %s\n   before stood on: %s (path %s)\n   HEAD stood on:   %s (path %s)\n", $1, $2, $3, $4, $5, $6, changed, agrees, $7, $9, $8, $10 }' > .run-unit/unit444-lineup.txt
grep -c "" .run-unit/unit444-lineup.txt
grep -A2 "CHANGED" .run-unit/unit444-lineup.txt | cut -c1-420
