#!/bin/sh
# unit 459 - V-11: every recording's row before and after, on the four metrics; prints only rows that moved.
# Usage: sh .run-unit/unit459-v11.sh <before-suffix> <after-suffix>
# Row: row | recording | set | key | sure emitted | sure wrong | sure added | CER | sent | sure right | coverage | placeholders | below high | words | inserted | deleted | WBE
cd /c/Source/HamLet || exit 1
export LC_ALL=C
for s in "$1" "$2"
do
  grep -a "^ row | " .run-unit/unit459-metrics-$s.txt | awk -F'|' '$4 ~ /^ (inferred|exact) $/ {gsub(/ /, "", $2); print $2 " | emitted " $5 "| wrong " $6 "| added " $7 "| sent " $9 "| right " $10 "| ins " $15 "| del " $16}' | sed 's/  */ /g' | sort > .run-unit/unit459-v11-$s.txt
done
echo "== rows that moved, before (<) and after (>)"
diff .run-unit/unit459-v11-$1.txt .run-unit/unit459-v11-$2.txt
wc -l .run-unit/unit459-v11-$1.txt .run-unit/unit459-v11-$2.txt
