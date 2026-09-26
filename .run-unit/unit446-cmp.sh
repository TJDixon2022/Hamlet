#!/bin/sh
# unit 446 - compare the per-condition rows of two metrics printouts.
# Usage: sh .run-unit/unit446-cmp.sh <before.txt> <after.txt>
cd /c/Source/HamLet || exit 1
grep "condition | " "$1" | awk -F' [|] ' '$4 != "condition" { print $2 " | " $3 " | " $4 " | " $5 " | " $6 " | " $7 " | " $8 " | " $10 }' > .run-unit/unit446-cmp-a.tmp
grep "condition | " "$2" | awk -F' [|] ' '$4 != "condition" { print $2 " | " $3 " | " $4 " | " $5 " | " $6 " | " $7 " | " $8 " | " $10 }' > .run-unit/unit446-cmp-b.tmp
paste -d '\n' .run-unit/unit446-cmp-a.tmp .run-unit/unit446-cmp-b.tmp | head -40
