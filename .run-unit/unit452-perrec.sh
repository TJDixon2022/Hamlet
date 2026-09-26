#!/bin/sh
# unit 452 - per recording, words sent, boundaries inserted and deleted, MET-WBE, from one metrics printout.
# Usage: sh .run-unit/unit452-perrec.sh <metrics.txt> [all]
cd /c/Source/HamLet || exit 1
grep -a "^ *row | " "$1" | grep -av "row | recording" | awk -F' [|] ' -v all="$2" 'NF >= 17 && $5 ~ /^[0-9]+$/ && (all == "all" || $15 + $16 > 0) { printf "%s | %s | %s | words %s | inserted %s | deleted %s | MET-WBE %s\n", $2, $3, $4, $14, $15, $16, $17 }' | sort -u
